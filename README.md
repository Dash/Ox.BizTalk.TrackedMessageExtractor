# Ox.BizTalk.TrackedMessageExtractor #
This utility is to extract a bunch of messages from the BizTalk tracking database on-bulk.

I find it an invaluable tool for when things goes titsup and you need to replay messages in a BizTalk environment.

For this to work, you need to have message body tracking on somewhere relevant (initial messages are a really good thing to track).

# Basic Usage #
Get yourself a list of message ids (guids) from your tracking database that meet your requirements.

```
SELECT TOP 10 [MessageInstance/InstanceID] FROM BizTalkDTADb..dtav_MessageFacts WHERE ... whatever ...
```

And pop them in a text file, one line per message id:

```
43336CC0-9D4D-4D27-A533-F3A45798EFA5
D11E86E8-0584-4072-851E-555337320B98
E8807B03-D9BE-4E95-9404-9BF7786D0FD0
1FA28300-5410-404A-9A92-6998B6DA7E5C
6822D96A-AC54-4FED-AD2C-596C6CABADAE
31844458-0C06-44CF-A556-78539359A6A1
```

Run the program and either provide your text file path when prompted.

# Configuration #
The program will attempt to use WMI on the local machine to get the relevant BizTalk databases.  If the local machine isn't connected to a BizTalk group this will fail and prompt you for the details.

You will, naturally, need relevant permissions on the BizTalk databases to utilise this tool.

# Dependencies #
Microsoft BizTalk server needs to be installed for this tool to work/build as it relies on BizTalk management DLLs.

Being a BizTalk component, this is obviously built against .NET Framework and not .NET Core.

# Arguments #

You can pass arguments into the application to kick everything off with one call.

```
Usage:
  --in=PATH            Line delimetered file of BizTalk message ids
  --mgmthost=SQLHOST   Hostname of Mgmt SQL server/instance to connect to
  --dtahost=SQLHOST    Hostname of the DTA SQL server/isntance
  --mgmtdb=DB          Database name of BizTalkMgmtDb
  --dtadb=DB           Database name of the BizTalkDTADb
  --out=PATH           Alternative output directory
  --nameschema=URI     Context namespace to use for deriving original filename
  --nameproperty=NAME  Context property name to use for deriving original filename
  --quit               Do not prompt for program closure at end
```

Normally for a quick/unattended run through you should be able to use:

```
extractmessages.exe --in=messages.txt --out=.\ --quit
```

This will parse the message ids supplied in the file messages.txt and output to the current working directory before ending the program without holding the application open.