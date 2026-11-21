import test from 'node:test';
import assert from 'node:assert/strict';
import {normalizeIl} from './normalize-il.mjs';
test('ignores only known volatile ildasm diagnostics and line encoding',()=>{
 assert.equal(normalizeIl('\ufeff// MVID: {a}\r\n// Image base: 0x1\r\n// WARNING: Created Win32 resource file C:/a.res\r\n.ret\r\n'),normalizeIl('// MVID: {b}\n// Image base: 0x2\n// WARNING: Created Win32 resource file D:/b.res\n.ret\n'));
});
test('retains architecture directives, opcodes, strings and other warnings',()=>{
 for(const line of ['.imagebase 0x10000000','.corflags 0x1','IL_0000: ldc.i4.1','IL_0001: ldstr "// MVID: original text"','// WARNING: unresolved reference'])assert.equal(normalizeIl(line),line);
 assert.notEqual(normalizeIl('IL_0000: ldc.i4.1'),normalizeIl('IL_0000: ldc.i4.2'));
});
