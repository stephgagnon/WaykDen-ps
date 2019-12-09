Remove-Item frontend -Recurse -Force  -ErrorAction SilentlyContinue
Remove-Item code -Recurse -Force  -ErrorAction SilentlyContinue
Remove-Item login -Recurse -Force  -ErrorAction SilentlyContinue
Remove-Item src -Recurse -Force  -ErrorAction SilentlyContinue
mkdir frontend
mkdir code

$Location = Get-Location

if (Get-Command "7z.exe" -ErrorAction SilentlyContinue) { 
	Start-Process -FilePath '7z.exe' -ArgumentList "x $Location/frontend.7z -o$Location/frontend -r" -Wait
}else{
	throw "You need to install 7zp https://www.7-zip.org/download.html"
}

if (Get-Command "git" -ErrorAction SilentlyContinue) { 
	#submodule src
	Set-Location code
	$Location = Get-Location
	git submodule add git@bitbucket.org:devolutions/lucid.git
	Rename-Item -Path $Location/lucid/ -NewName src -Force
	Set-Location src
	git checkout 11321ade5144ead3fd502b2f76694ec698874332
	Set-Location ..
	Set-Location ..
	$Location = Get-Location
	Copy-Item -Path $Location/code/src -Destination $Location -Force -Recurse

	#submodule login
	Start-Process -FilePath 'git' -ArgumentList "submodule add git@bitbucket.org:devolutions/lucid-login.git" -Wait
	$Location = Get-Location
	Rename-Item -Path $Location/lucid-login/ -NewName login -Force
	Set-Location login
	Start-Process -FilePath 'git' -ArgumentList "checkout 11321ade5144ead3fd502b2f76694ec698874332" -Wait
	Set-Location ..
}else{
	throw "You need to install git"
}

docker build -t devolutions/den-lucid:3.6.5-servercore-1803 ./