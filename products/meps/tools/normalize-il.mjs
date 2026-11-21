// Remove only ildasm-generated diagnostic comments, never IL directives or string contents.
export function normalizeIl(text){
 return text.replace(/^\uFEFF/,'').split(/\r?\n/).filter(line=>
  !/^\s*\/\/ (MVID:|Image base:|WARNING: Created Win32 resource file )/.test(line)
 ).join('\n');
}
