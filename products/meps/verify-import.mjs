import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import {fileURLToPath} from 'node:url';
const base=path.dirname(fileURLToPath(import.meta.url));
const manifest=JSON.parse(fs.readFileSync(path.join(base,'import-manifest.json'),'utf8'));
const root=path.join(base,'snapshot');
const expected=new Set(manifest.entries.map(e=>e.path));
let count=0,bytes=0;
for(const e of manifest.entries){
 const data=fs.readFileSync(path.join(root,e.path));
 if(data.length!==e.size||crypto.createHash('sha256').update(data).digest('hex')!==e.sha256)throw Error('Mismatch: '+e.path);
 count++;bytes+=data.length;
}
function walk(dir){for(const e of fs.readdirSync(dir,{withFileTypes:true})){const f=path.join(dir,e.name);if(e.isDirectory())walk(f);else if(!expected.has(path.relative(root,f).replaceAll('\\','/')))throw Error('Unlisted file: '+f);}}
walk(root);
console.log(`PASS: ${count} files, ${bytes} bytes match SHA-256 manifest; no unlisted files.`);
