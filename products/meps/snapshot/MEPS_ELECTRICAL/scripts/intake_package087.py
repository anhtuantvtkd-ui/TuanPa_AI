import json,subprocess
from pathlib import Path
from datetime import datetime
w=Path.cwd();p=w/'PROJECTS/MEPS_ELECTRICAL';r=w/'REPOSITORIES/MEPS';t=r/'.AI/tasks/MEPS-PORTABLE-087'
def read(f):return json.loads(f.read_text(encoding='utf-8-sig'))
def save(f,x):f.write_text(json.dumps(x,ensure_ascii=False,indent=2),encoding='utf-8')
locks=[w/'AI_CONTROL/LOCKS/WORKSPACE.json',p/'.AI/LOCK.json',r/'.AI/LOCK.json']
for f in locks:assert not read(f).get('locked'),f
base=subprocess.check_output(['git','rev-parse','HEAD'],cwd=r,text=True).strip()
dirty=subprocess.check_output(['git','status','--porcelain'],cwd=r,text=True)
subprocess.run(['git','switch','-c','codex/portable-087'],cwd=r,check=True)
t.mkdir();q=p/'TESTS/PACKAGE087';q.mkdir()
task=dict(task_id=t.name,objective='Package latest MEPS for transfer to another Windows AutoCAD2021 machine',base_commit=base,branch='codex/portable-087',scope=['scripts/package087','tests/package087','docs/portable087','.AI/tasks/MEPS-PORTABLE-087','project TESTS/PACKAGE087 WORK/RELEASE087 OUTPUT archive manifests','workspace control'],execution='GPT_DIRECT_PACKAGING_SELF_REVIEW; no product code edits',preserved_dirty=dirty,source_dll_sha256='3C2DAFCF4A76EBEE40F512F6FAC192C28C84B35B9D39B57F80C846CBA96B0493',source_scope='latest reviewed086binary; no CAD live reload',acceptance=['all runtime dependencies portable','library catalogs and hashes valid','isolated CAD native pilot on owned scratch','clean ZIP manifest','transfer instructions and known limitations'],known_limits=['086 lighting region topology unresolved; do not claim repaired','Cross-provider086 review pending429; not overall production acceptance','Second machine not available; test relocation locally'],lessons={'MEPS-QA-003':'Owned CoreConsole one at time, no live CAD','MEPS-QA-033REALCATALOG':'Use101entry actual library with recovered blocks','DELIVERY16-20':'One current package, preserve user data and archive replacedinstaller'},next_allowed_step='STAGE_VALIDATE_PORTABLE_PACKAGE')
save(t/'TASK.json',task);save(t/'STATE.json',dict(task,status='IN_PROGRESS'));save(q/'TASK.json',task)
for f in locks:save(f,dict(locked=True,owner='GPT',task_id=t.name,scope=task['scope'],updated_at=datetime.now().isoformat()))
save(t/'GAP_REGISTER.json',dict(GAP_SCAN='OUTPUT076 obsolete versus repo086; runtime library101versus repo98',CONFLICT_SCAN='Preserve three existing uncommitted086records; CAD untouched',REQUIREMENT_COMPLETENESS_CHECK='TargetAutoCAD2021 established; transfer not full product repair',DEPENDENCY_SCAN='SDNL runtime and user configs outside repo must be bundled locally only',REVIEW_SCAN='Product086 independentGPTpass, providerpending; packaging selfreview',GAP_SCAN_AGAIN='Pending native pilot and asset resolution'))
cw=read(w/'AI_CONTROL/CURRENT_WORK.json');cw.update(active_task=t.name,task_path=str(t.relative_to(w)),status='MEPS_PACKAGING',next_allowed_step=task['next_allowed_step']);save(w/'AI_CONTROL/CURRENT_WORK.json',cw)
print(t)
