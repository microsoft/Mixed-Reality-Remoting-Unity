$unityPath = "C:\Program Files\Unity\Hub\Editor\2020.3.20f1\Editor\Unity.exe"
$buildPath = [IO.Path]::Combine($PSScriptRoot, 'build')

# Build APK
$apkBuildArgs = @("-batchmode", "-projectpath ./MR-Remoting-Android-App.Unity", "-executeMethod BuildApk.Build", "-TargetFolder ""$buildPath""", "-logFile build/build-apk.log")
Start-Process -FilePath $unityPath -ArgumentList $apkBuildArgs -Wait

#  Build UPM
$upmBuildArgs = @("-batchmode", "-projectpath ./MR-Remoting-Host.Unity", "-executeMethod MRMobileRemoting.UPMTools.CreatePackage", "-TargetFolder ""$buildPath""", "-logFile build/build-unity-package.log")
Start-Process -FilePath $unityPath -ArgumentList $upmBuildArgs -Wait