@echo off
cloc . -exclude-dir=BuildTools,bin,obj,packages,x86,x64,.vs,Debug,Release,debug,release -not-match-f="lodepng\..*" --exclude-lang=XSD,ASP.Net,XML,"MSBuild scripts","MSBuild script",CSS,HTML
pause