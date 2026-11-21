import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import {spawnSync} from 'node:child_process';
import {fileURLToPath} from 'node:url';
import {normalizeIl} from './normalize-il.mjs';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
const artifactRoot=path.join(root,'.artifacts');
const ildasm=process.env.MEPS_ILDASM||'C:/Program Files (x86)/Microsoft SDKs/Windows/v10.0A/bin/NETFX 4.8 Tools/x64/ildasm.exe';
if(!fs.existsSync(ildasm))throw Error('Set MEPS_ILDASM to .NET Framework ildasm.exe.');
function buildInput(name){if(!name||path.basename(name)!==name)throw Error('Pass build directory names from .artifacts.');const dir=path.join(artifactRoot,name),e=JSON.parse(fs.readFileSync(path.join(dir,'build-result.json'),'utf8'));if(e.exitCode!==0)throw Error('Build failed: '+name);return path.join(dir,'TVXD.Meps.dll');}
const inputs={v4:path.join(root,'snapshot/TVXD.BG V4/MEPS060/TVXD.Meps.dll'),il:buildInput(process.argv[2]),csharp:buildInput(process.argv[3])};
const out=fs.mkdtempSync(path.join(artifactRoot,'compare-'));
const hash=b=>crypto.createHash('sha256').update(b).digest('hex');
const views={},summary={normalization:'Remove only ildasm comments for MVID, Image base and Created Win32 resource file path; normalize BOM and line endings. Preserve every other line. Compare extracted resource bytes separately.',ildasmSha256:hash(fs.readFileSync(ildasm)),assemblies:{}};
for(const [label,file] of Object.entries(inputs)){
 const dir=path.join(out,label);fs.mkdirSync(dir);const il=path.join(dir,'assembly.il');
 const r=spawnSync(ildasm,[file,'/nobar','/utf8','/out='+il],{cwd:dir,encoding:'utf8',maxBuffer:4*1024*1024});
 if(r.status!==0)throw Error('ildasm failed: '+label+' '+r.stdout+' '+r.stderr);
 const text=normalizeIl(fs.readFileSync(il,'utf8'));views[label]=text;
 const binary=fs.readFileSync(file),pe=binary.readUInt32LE(0x3c);
 const classes=[...text.matchAll(/^\.class (.+)$/gm)].map(m=>m[1]);
 const res=fs.readdirSync(dir).filter(f=>f!== 'assembly.il').map(f=>({file:f,sha256:hash(fs.readFileSync(path.join(dir,f)))}));
 summary.assemblies[label]={sha256:hash(binary),bytes:binary.length,machine:binary.readUInt16LE(pe+4),topLevelClassDeclarations:classes.length,classes,normalizedIlSha256:hash(text),extractedResources:res};
}
summary.ilTextMatchesV4=views.il===views.v4;
summary.ilResourcesMatchV4=JSON.stringify(summary.assemblies.il.extractedResources)===JSON.stringify(summary.assemblies.v4.extractedResources);
summary.ilMachineMatchesV4=summary.assemblies.il.machine===summary.assemblies.v4.machine;
summary.csharpTextMatchesV4=views.csharp===views.v4;
const csharpTypes=new Set(summary.assemblies.csharp.classes);
summary.v4DeclarationsMissingFromCsharp=summary.assemblies.v4.classes.filter(c=>!csharpTypes.has(c));
for(const label of ['il','csharp']){const a=views.v4.split('\n'),b=views[label].split('\n');const i=a.findIndex((l,i)=>l!==b[i]);summary[label+'FirstTextDifference']=i<0?(a.length===b.length?null:{line:Math.min(a.length,b.length)+1}):{line:i+1,v4:a[i],candidate:b[i]};}
summary.nativeTested=false;
fs.writeFileSync(path.join(out,'comparison.json'),JSON.stringify(summary,null,2)+'\n');
console.log(JSON.stringify({out,...summary,assemblies:Object.fromEntries(Object.entries(summary.assemblies).map(([k,v])=>[k,{...v,classes:undefined}])),v4DeclarationsMissingFromCsharp:summary.v4DeclarationsMissingFromCsharp.length},null,2));
if(!summary.ilTextMatchesV4||!summary.ilResourcesMatchV4||!summary.ilMachineMatchesV4)process.exitCode=1;
