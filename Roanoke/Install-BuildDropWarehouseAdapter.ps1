[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [uri]
    $CollectionUri,

    [switch]
    $RemoveJob,

    [switch]
    $RemovePlugin
)

Set-StrictMode -Version Latest
$script:ErrorActionPreference = 'Stop'

$PSScriptRoot = $MyInvocation.MyCommand.Path | Split-Path

$AppTierPath = (Get-ItemProperty -Path HKLM:\SOFTWARE\Microsoft\TeamFoundationServer\11.0\InstalledComponents\ApplicationTier).InstallPath
$PluginsPath = Join-Path -Path $AppTierPath -ChildPath TFSJobAgent\plugins

if (-not $RemovePlugin) {

    if (Test-Path -Path $PSScriptRoot\roanoke.dll) {

        Stop-Service -Name TFSJobAgent
        Copy-Item -Path $PSScriptRoot\roanoke.dll -Destination $PluginsPath -Force
        Start-Service -Name TFSJobAgent

    } else {
        Write-Warning "File 'roanoke.dll' not found. Skipping install step."
    }

}

Add-Type -AssemblyName 'Microsoft.TeamFoundation.Client, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'

$Collection = New-Object -TypeName Microsoft.TeamFoundation.Client.TfsTeamProjectCollection -ArgumentList $CollectionUri
$Collection.EnsureAuthenticated()
$JobService = $Collection.GetService([Microsoft.TeamFoundation.Framework.Client.ITeamFoundationJobService])

$JobName = 'Build Drop Warehouse Sync'
$JobExtensionName = 'Roanoke.BuildDropWarehouseSyncJobExtension'
$JobData = ([xml]'<Data>JobCategories=Warehouse;</Data>').DocumentElement

$Job = $JobService.QueryJobs() | Where-Object { $_.ExtensionName -eq $JobExtensionName } | Select-Object -First 1

if (-not $Job) {
    $Job = New-Object -TypeName Microsoft.TeamFoundation.Framework.Client.TeamFoundationJobDefinition -ArgumentList $JobName, $JobExtensionName, $JobData
    $Schedule = New-Object -TypeName Microsoft.TeamFoundation.Framework.Client.TeamFoundationJobSchedule -ArgumentList ([DateTime]::UtcNow),(2*60)
    $Job.Schedule.Add($Schedule)
    $JobService.UpdateJob($Job)
} elseif ($RemoveJob) {
    $JobService.DeleteJob($Job)

}

if ($RemovePlugin) {

    if (Test-Path -Path $PluginsPath\roanoke.dll) {

        Stop-Service -Name TFSJobAgent
        Remove-Item -Path $PluginsPath\roanoke.dll
        Start-Service -Name TFSJobAgent

    }

}
