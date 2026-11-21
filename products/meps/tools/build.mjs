import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import {spawnSync} from 'node:child_process';
import {fileURLToPath} from 'node:url';

const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
const snapshot=path.join(root,'snapshot');
const mode=process.argv[2];
if(!['csharp','il'].includes(mode))throw Error('Usage: node products/meps/tools/build.mjs csharp|il');
if(process.platform!=='win32')throw Error('Requires Windows and .NET Framework 4.x build tools.');
const framework=process.env.MEPS_FRAMEWORK_DIR||path.join(process.env.WINDIR||'C:/Windows','Microsoft.NET/Framework64/v4.0.30319');
const cad=process.env.MEPS_CAD_DIR||'C:/Program Files/Autodesk/AutoCAD 2021';
const walk=d=>fs.readdirSync(d,{withFileTypes:true}).flatMap(e=>e.isDirectory()?walk(path.join(d,e.name)):[path.join(d,e.name)]);
const hash=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
const requireFile=f=>{if(!fs.existsSync(f))throw Error('Missing prerequisite: '+f);return f;};
const compiler=requireFile(path.join(framework,mode==='il'?'ilasm.exe':'csc.exe'));
const defines=['WIRES010','STANDARDS012','IDENTITY015','CABLES016','WIRECOMMIT018','WIRESCROLL018','MODELLIB019','FLEX022','LIBUPDATE023','LAYOUT030','UI031','CIRCUIT032','METADATA032','TAG033','BDGKL034','ROUTE034','ROOMCAD037'];
let inputs,refs=[];
if(mode==='csharp'){
 inputs=walk(path.join(snapshot,'MEPS_ELECTRICAL/src')).filter(f=>f.endsWith('.cs')).sort();
 if(!inputs.length)throw Error('No C# inputs');
 refs=['AcMgd.dll','AcDbMgd.dll','AcCoreMgd.dll'].map(n=>requireFile(path.join(cad,n)));
 refs.push(...['WindowsBase.dll','PresentationCore.dll'].map(n=>requireFile(path.join(framework,'WPF',n))));
}else inputs=['meps_final.il','meps_full.res'].map(n=>requireFile(path.join(snapshot,n)));
fs.mkdirSync(path.join(root,'.artifacts'),{recursive:true});
const outDir=fs.mkdtempSync(path.join(root,'.artifacts',mode+'-'));
const output=path.join(outDir,'TVXD.Meps.dll');
let args;
if(mode==='csharp'){
 const system=['System','System.Core','System.Data','System.Drawing','System.Windows.Forms','System.Web.Extensions','System.IO.Compression','System.IO.Compression.FileSystem','System.Xml','System.Xml.Linq'];
 const options=['/target:library','/codepage:65001','/nologo','/optimize+','/platform:x64','/d:'+defines.join(';'),'/out:"'+output+'"',...system.map(n=>'/r:'+n+'.dll'),...refs.map(f=>'/r:"'+f+'"'),...inputs.map(f=>'"'+f+'"')];
 const rsp=path.join(outDir,'build.rsp');fs.writeFileSync(rsp,'\ufeff'+options.join('\r\n'));args=['@'+rsp];
}else{
 // Preserve the PE32 IL-only target declared in the stored IL; do not invent an x64 conversion.
 args=[inputs[0],'/dll','/quiet','/output='+output,'/resource='+inputs[1]];
}
const result=spawnSync(compiler,args,{cwd:outDir,encoding:'utf8',maxBuffer:16*1024*1024});
const log=(result.stdout||'')+(result.stderr||'')+(result.error?'\n'+result.error.message:'');
fs.writeFileSync(path.join(outDir,'build.log'),log);
const evidence={mode,compiler,compilerSha256:hash(compiler),args,exitCode:result.status,sourceFiles:inputs.map(f=>({path:path.relative(root,f).replaceAll('\\','/'),sha256:hash(f)})),references:refs.map(f=>({path:f,sha256:hash(f)})),defines:mode==='csharp'?defines:[],output,outputSha256:fs.existsSync(output)?hash(output):null,nativeTested:false};
fs.writeFileSync(path.join(outDir,'build-result.json'),JSON.stringify(evidence,null,2)+'\n');
console.log(log);console.log(JSON.stringify({mode,exitCode:result.status,sourceCount:inputs.length,outDir,outputSha256:evidence.outputSha256},null,2));
process.exitCode=result.status===0?0:1;
