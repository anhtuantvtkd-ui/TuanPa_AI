import {test} from "node:test";
import assert from "node:assert/strict";
import {requireOwner,safePath,readRepo,askLlama,OWNER_ID,MODEL} from "./services.mjs";
test("only numeric owner ID is accepted",()=>{
 assert.doesNotThrow(()=>requireOwner({userId:OWNER_ID}));
 for(const p of [undefined,{}, {userId:1},{userId:String(OWNER_ID)},{login:"anhtuantvtkd-ui"}])assert.throws(()=>requireOwner(p));
});
test("reject traversal and control characters",()=>{
 for(const p of ["../secret","/etc/passwd","a/../b","a\\b","a\u0000b","a/./b"])assert.throws(()=>safePath(p));
 assert.equal(safePath("docs/hello world.md"),"docs/hello%20world.md");
});
test("repo target fixed, directory fields limited",async()=>{
 const r=await readRepo("",async(url,opt)=>{assert.equal(url,"https://api.github.com/repos/anhtuantvtkd-ui/TuanPa_AI/contents/?ref=main");assert.equal(opt.redirect,"error");return Response.json([{name:"README.md",path:"README.md",type:"file",size:10,secret:"not exposed"}]);});
 assert.equal(r.entries[0].secret,undefined);
});
test("decode UTF8 with SHA provenance",async()=>{
 const text="Xin chào";const r=await readRepo("README.md",async()=>Response.json({type:"file",encoding:"base64",size:20,path:"README.md",sha:"abc",content:Buffer.from(text).toString("base64")}));
 assert.equal(r.text,text);assert.equal(r.sha,"abc");
});
test("reject big files and upstream errors",async()=>{
 await assert.rejects(()=>readRepo("big",async()=>Response.json({type:"file",encoding:"base64",size:64001})));
 await assert.rejects(()=>readRepo("missing",async()=>new Response("",{status:404})));
});
test("Llama bounded request, real response only",async()=>{
 let calls=0;const ai={run:async(model,args)=>{calls++;assert.equal(model,MODEL);assert.equal(args.max_tokens,768);return {response:"ok"};}};
 assert.equal((await askLlama(ai,"hello")).answer,"ok");
 await assert.rejects(()=>askLlama(ai,"x".repeat(6001)));assert.equal(calls,1);
 await assert.rejects(()=>askLlama({run:async()=>({})},"hello"));
});
