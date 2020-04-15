# Protohan
Registering and executing custom protocols.

A simple tool to run custom protocols in, for example, the browser.

### Basics

To register a protocol and to use it, enter the following in the command prompt. Elevated rights could be needed, since you are going to write to the Windows registry.

```
ProtoHan.exe -register myprotocol "c:\path\to\app.exe"
```

Then run in browser:
```
myprotocol:\\some_string
```

This will open the application stated when registering.

Then run in browser:
```
anotherprotocol:\\some_string
```

Nothing will happen since the system does not know the protocol.
