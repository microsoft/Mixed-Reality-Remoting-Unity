$unityPath = "C:\Program Files\Unity\Hub\Editor\2020.3.20f1\Editor\Unity.exe"

$outputPath = [IO.Path]::Combine($PSScriptRoot, "build")

$apkProjectPath = [IO.Path]::Combine($PSScriptRoot, "MR-Remoting-Android-App.Unity")
$upmProjectPath = [IO.Path]::Combine($PSScriptRoot, "MR-Remoting-Host.Unity.Unity")

$apkLogPath = [IO.Path]::Combine($outputPath, "build-apk.log")
$upmLogPath = [IO.Path]::Combine($outputPath, "build-unity-packages.log")

# Build APK
$apkBuildArgs = @("-batchmode", "-projectpath ""$apkProjectPath""", "-executeMethod BuildApk.Build", "-TargetFolder ""$outputPath""", "-logFile ""$apkLogPath""")
Write-Output "Building Android companion app with args: "
Write-Output $apkBuildArgs
Start-Process -FilePath $unityPath -ArgumentList $apkBuildArgs -Wait

Write-Output ""

#  Build UPM
$upmBuildArgs = @("-batchmode", "-projectpath ""$upmProjectPath""", "-executeMethod MRMobileRemoting.UPMTools.CreatePackage", "-TargetFolder ""$outputPath""", "-logFile ""$upmLogPath""")
Write-Output "Building Unity package with args: "
Write-Output $upmBuildArgs
Start-Process -FilePath $unityPath -ArgumentList $upmBuildArgs -Wait