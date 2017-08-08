msbuild "/t:Restore;Pack" .\NetFusion.sln /p:VersionPrefix=2.0.45 
for /r ./src %%x in (*.nupkg) do copy "%%x" ..\_packages



