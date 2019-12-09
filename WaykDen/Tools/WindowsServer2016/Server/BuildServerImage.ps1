Remove-Item frontend -Recurse -Force  -ErrorAction SilentlyContinue

$Location = Get-Location

if (Get-Command "7z.exe" -ErrorAction SilentlyContinue) { 
	Start-Process -FilePath '7z.exe' -ArgumentList "x $Location/frontend.7z -o$Location/frontend -r" -Wait
}else{
	throw "You need to install 7zp https://www.7-zip.org/download.html"
}

docker build -t devolutions/den-server:1.8.0-servercore-1803 ./