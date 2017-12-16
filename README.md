# ![µGoverner](https://cloud.githubusercontent.com/assets/544444/9152442/f85bddee-3e28-11e5-82b6-450eeb9028fe.png) [![Build status](https://ci.appveyor.com/api/projects/status/wamt000xkykoe9l5?svg=true)](https://ci.appveyor.com/project/StevenThuriot/ugovernor)
a µTorrent cli

# Usage

µGovernor supports both authenticating using a webtoken or not. For security reasons, it's advised to keep the webtoken turned on.

A few settings will always need to be passed to µGovernor:
- The username
- The password
- And obviously: the hostname.

Since it wouldn't be secure keeping these in the "run program" feature of µTorrent, µGovernor supports saving these in an AES-256 encrypted file. The file is encrypted using a fingerprint built from your pc's specs, so it can only be decompiled on your own hardware, keeping your settings safe.

Create such a file can be done using the cli:

```
uGovernor -save user USERNAME password PASSWORD host http://HOSTNAME:PORT
```

After the file is created, it won't be necesairy anymore to pass these settings to the cli. If you do pass any of these settings, they will overule the ones in the config file (e.g. for temporarely logging into a different server than your default one).

After this, the cli can be used by passing it startup arguments, e.g.

```
uGovernor -start
```

This command will start all the torrents currently in your list.

# Available Commands
The following actions can be used:

## -host [VALUE]
Sets the server to connect to. 
e.g. `-host http://myserver.com:8080/`
It's not necessairy to add `/gui/`, but won't hurt either if you do.

## -user [VALUE]
Sets the username to log in with.

## -password [VALUE]
Sets the password to log in with.

## -hash [VALUE]
Sets the torrent hash we want to use our passed commands on. When ommited, the cli will execute the commands on all torrents currently in the list.

This command can be used multiple times when you want to use the same command on several hashes at once.

```
uGovernor -start -hash HASH1 -hash HASH2
```

## -noTokenAuth
Disables token auth, when you have disabled it in the webgui as well. 
Strongly advised not to do this!

## -debug
All actions will be written/appended to debug.log

## -ui
Shows a console UI. When started from a console, it will attach to parent instead.

## -list[public|private]
List all torrents and their hashes. A filter can be used (public or private). The filter is optional. Not supplying it will result in all torrents being printed.

When `-ui` is not supplied, it will try to open a UI or attach to a parent UI anyway.

## -start[_ifprivate|_ifpublic]
Start the torrent

## -stop[_ifprivate|_ifpublic]
Stop the torrent

## -forcestart[_ifprivate|_ifpublic]
Force Start the torrent

## -pause[_ifprivate|_ifpublic]
Pause the torrent

## -unpause[_ifprivate|_ifpublic]
Unpause the torrent

## -recheck[_ifprivate|_ifpublic]
Recheck the torrent

## -remove[_ifprivate|_ifpublic]
Remove the torrent from the list. This will use your default removal setting.

## -removeData[_ifprivate|_ifpublic]
Remove both the torrent and the data from the list.

## -label[_ifprivate|_ifpublic] [VALUE]
Set the passed label to the torrent. If it doesn't exist yet, it will be created.

## -removeLabel[_ifprivate|_ifpublic]
Remove the torrent's label.

## -setPrio[_ifprivate|_ifpublic] [VALUE]
Set the torrents priority.

## -setProperty[_ifprivate|_ifpublic] [NAME] [VALUE]
A sort of catch all command. When wanting to set any property, e.g. download path.

## -save [NAME] [VALUE] [NAME2] [VALUE2] .... [NAME_N] [VALUE_N]
Securely saves the passed values to a file.
Currently, only user, password and host are used.

When adding or changing a property, it is not needed to supply all the previously saved (unaltered) properties again. If the property already existed in the file, it will be overridden, if not, it will be added. If you want a certain property deleted, you'll have to fully recreate the `cfg` file.

## -add [HASH]
Add a torrent to the list by passing a hash. The torrent will be resolved using a magnet link.

## -addResolved [HASH]
Add a torrent to the list by passing a hash. The torrent will be resolved using torcache.


# Private and Public Torrents

All commands that have the `[_ifprivate|_ifpublic]` are in fact, three forms of that command, e.g:

This command will start all torrents in the list:

```
uGovernor -start
```

While this command will only start the private torrents in the list:

```
uGovernor -start_ifprivate
```

Obviously, this command will start all the public torrents in the list:

```
uGovernor -start_ifpublic
```

# Combining Commands

.. is also a possibility!

For instance, we can stop all public torrents, start all private ones, remove all labels from the public ones, set a label to all private ones and recheck all of them, wether they are public or not.

```
uGovernor -stop_ifpublic -start_ifprivate -removeLabel_ifpublic -label_ifprivate MyLabel -recheck
```


# Legal Information

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
