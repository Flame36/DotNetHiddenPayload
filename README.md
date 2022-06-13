This repo is a simple proof of concept on how to store data inside the same file as your exe, but in a way that lets it be modified from external sources.

## Pros/Cons
### Pros
+ Variable length
+ Can be modified externally
### Cons
- Cannot write changes on runtime *

\* There are fixes, e.g. using a helper that kills the process or [detaching the process from the file](http://www.johnfindlay.plus.com/lcc-win32/asm/SelDelNT.htm)

## How it works
A packet is formed from:  
* **A header:** that prevents finding a garbage payload in an exe where there isn't one  
* **The payload length:** a signed 64 bit integer  
* **The payload**  
  
The packet is then reversed and appended to the end of the file  
  
To then get the payload you read the file stream in reverse, check if the header matches, read the payload length, and finally read the payload

