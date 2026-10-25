import test from 'node:test';
import assert from 'node:assert/strict';
import { readFile, mkdtemp, mkdir, writeFile, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { validateTask, checkDirectory } from './workflow-check.mjs';
const template=JSON.parse(await readFile(new URL('./TASK.template.json',import.meta.url),'utf8'));
const commit='a'.repeat(40);
function approved() { return {...structuredClone(template),status:'APPROVED',writer:'Argon-session-1',reviewer:'Reviewer-session-2',base_commit:'b'.repeat(40),result_commit:commit,branch:'task/TASK-0001-example',scope:['example'],sources:['synthetic independent reference'],missing_inputs:[],acceptance_criteria:['reference match'],required_checks:['unit'],evidence:[{check:'unit',status:'PASS',commit,reference:'test-report.md'}],review:{reviewer:'Reviewer-session-2',independent:true,reviewed_commit:commit,verdict:'APPROVED',findings:[]},approval:{by:template.owner,commit,reference:'owner approval record'}}; }
test('template records missing inputs without claiming acceptance',()=>assert.deepEqual(validateTask(template),[]));
test('reject malformed record',()=>assert.ok(validateTask(null).length));
test('valid approval evidence structure',()=>assert.deepEqual(validateTask(approved()),[]));
test('stale test evidence blocks approval',()=>{const t=approved();t.evidence[0].commit='c'.repeat(40);assert.ok(validateTask(t).some(e=>e.includes('PASS')));});
test('stale review blocks approval',()=>{const t=approved();t.review.reviewed_commit='c'.repeat(40);assert.ok(validateTask(t).includes('review current commit'));});
test('writer cannot independently review high risk',()=>{const t=approved();t.review.reviewer=t.writer;assert.ok(validateTask(t).includes('independent high-risk review'));});
test('unresolved high finding blocks approval',()=>{const t=approved();t.review.findings=[{severity:'high',status:'open'}];assert.ok(validateTask(t).includes('resolve blocking findings'));});
test('missing owner evidence blocks approval',()=>{const t=approved();t.approval=null;assert.ok(validateTask(t).includes('owner approval evidence'));});
test('release requires artifact and rollback',()=>{const t=approved();t.status='RELEASED';assert.ok(validateTask(t).includes('artifact SHA256'));assert.ok(validateTask(t).includes('release rollback_guide'));});
test('directory catches malformed JSON and mismatched ID',async()=>{const root=await mkdtemp(join(tmpdir(),'tuanpa-workflow-'));try{assert.equal((await checkDirectory(root)).count,0);await mkdir(join(root,'work-items'));await writeFile(join(root,'work-items','TASK-0002.json'),JSON.stringify(template));await writeFile(join(root,'work-items','broken.json'),'{');const r=await checkDirectory(root);assert.equal(r.count,2);assert.equal(r.errors.length,2);}finally{await rm(root,{recursive:true,force:true});}});
