import { readdir, readFile } from 'node:fs/promises';
import { resolve, basename } from 'node:path';
import { pathToFileURL } from 'node:url';
const states = ['NEEDS_INPUT','IMPLEMENTING','BLOCKED','READY_FOR_REVIEW','APPROVED','RELEASED'];
const text = v => typeof v === 'string' && v.trim().length > 0;
const sha = v => typeof v === 'string' && /^[a-f0-9]{40}$/i.test(v);
export function validateTask(t) {
  const errors = [];
  const need = (ok,msg) => { if (!ok) errors.push(msg); };
  if (!t || typeof t !== 'object' || Array.isArray(t)) return ['task must be an object'];
  need(t.schema_version === 'TUANPA-ENG-1','schema_version');
  need(/^TASK-[0-9]{4,}$/.test(t.task_id || ''),'task_id');
  need(states.includes(t.status),'status');
  need(['low','medium','high'].includes(t.risk),'risk');
  for (const k of ['title','module','owner','writer','reviewer']) need(text(t[k]),k);
  for (const k of ['scope','out_of_scope','sources','missing_inputs','acceptance_criteria','required_checks','evidence'])
    need(Array.isArray(t[k]),k + ' must be an array');
  if (errors.length) return errors;
  if (t.status === 'NEEDS_INPUT') need(t.missing_inputs.length > 0,'record missing inputs');
  if (!['NEEDS_INPUT','BLOCKED'].includes(t.status)) {
    need(sha(t.base_commit),'base_commit must be full SHA');
    need(text(t.branch) && t.branch.startsWith('task/' + t.task_id + '-'),'task branch');
    need(t.writer !== 'unassigned','assign writer');
    for (const k of ['scope','sources','acceptance_criteria','required_checks']) need(t[k].length > 0,k + ' cannot be empty');
    need(t.missing_inputs.length === 0,'resolve missing inputs');
  }
  if (['READY_FOR_REVIEW','APPROVED','RELEASED'].includes(t.status)) {
    need(sha(t.result_commit),'result_commit must be full SHA');
    for (const check of t.required_checks) need(
      text(check) && t.evidence.some(e => e && e.check === check && e.status === 'PASS' && text(e.reference) && e.commit === t.result_commit),
      'missing current PASS evidence: ' + check);
  }
  if (['APPROVED','RELEASED'].includes(t.status)) {
    const r = t.review;
    need(r?.verdict === 'APPROVED' && r.reviewed_commit === t.result_commit && text(r.reviewer),'review current commit');
    if (t.risk === 'high') need(r?.independent === true && r.reviewer !== t.writer,'independent high-risk review');
    need(Array.isArray(r?.findings) && r.findings.every(f => f && (f.status === 'resolved' || !['critical','high'].includes(f.severity))),'resolve blocking findings');
    need(t.approval?.by === t.owner && t.approval?.commit === t.result_commit && text(t.approval?.reference),'owner approval evidence');
  }
  if (t.status === 'RELEASED') {
    const r=t.release;
    need(r?.source_commit === t.result_commit && text(r?.version),'release version and commit');
    need(Array.isArray(r?.artifacts) && r.artifacts.length > 0 && r.artifacts.every(a => text(a.path) && /^[a-f0-9]{64}$/i.test(a.sha256 || '')),'artifact SHA256');
    for (const k of ['installation_guide','rollback_guide','test_report']) need(text(r?.[k]),'release ' + k);
  }
  return errors;
}
export async function checkDirectory(root) {
  let files;
  try { files = await readdir(resolve(root,'work-items')); }
  catch (e) { if(e.code === 'ENOENT') return {count:0,errors:[]}; throw e; }
  const errors=[]; let count=0;
  for (const f of files.filter(f => f.endsWith('.json')).sort()) {
    count++;
    try {
      const t=JSON.parse(await readFile(resolve(root,'work-items',f),'utf8'));
      if(basename(f,'.json') !== t.task_id) errors.push(f + ': filename must match task_id');
      errors.push(...validateTask(t).map(e => f + ': ' + e));
    } catch(e) { errors.push(f + ': ' + e.message); }
  }
  return {count,errors};
}
if (process.argv[1] && import.meta.url === pathToFileURL(resolve(process.argv[1])).href) {
  const result = await checkDirectory(process.cwd());
  if(result.errors.length) { console.error(result.errors.join('\n')); process.exitCode=1; }
  else console.log('Workflow records valid: ' + result.count + (result.count ? '' : ' (no active tasks; no product acceptance implied)'));
}
