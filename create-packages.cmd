msbuild "/t:Restore;Pack" .\NetFusion.sln /p:VersionPrefix=2.0.50
for /r ./src %%x in (*.2.0.50.nupkg) do copy "%%x" ..\_packages



