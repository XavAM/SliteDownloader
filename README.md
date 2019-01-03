# SliteDownloader
Small software created for downloading embedded files from Slite Database back up.

## Context
This small has been created for users of [Slite.com](https://slite.com) documentation knowledge sharing app.
Following a database back up requested to the customer support, the archive will contains markdwon files organized in channels. Those markdown files may contain embedded file links which are the files dropped unto Slite by users.
The purpose of this software is to 
1. Identify those files
1. Download them onto a "Downloaded Files" Folder
1. Replace hyperlinks into markdown files by local links pointing to downloaded files.

## Use

1. Extract the Slite provided Archive in a folder
1. Drop the SliteDownloader Executable at the root of this folder. It should contain a folder named "channels" and another one named "users". 
1. Run the Exe File. It will prompt you for downloading found files and replacing hyperlinks.
Downladed files will be placed into a new folder and organized by channels. 

## Disclaimer

This software is not affiliated to [Slite.com](https://slite.com). I've developped it for an internal use, use it at your own risks.


