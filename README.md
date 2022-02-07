# MemBot

This is the telegram bot to store and receive mems, phrases, quotes, images etc\
Demo bot is running on Ubuntu(20.04) with .Net 6 and SQLite DB\
https://t.me/MemzzBot

## Supported commands
- /add - Add or update Mem
- /tags - Show available tags
- /help - Show help

### How to add mem
 1. Type or choose '/add' command
 2. Type the name of mem
 3. Type corresponding tags for this mem. They should starts with '#' and separated with '#'. Example: #movie #joke #quote
 4. If there is any media file for this mem - load it. Or just type command '/added'

### How to get random mem
 Just type tags, leaded by '#'. You can add more, than one tag, with '#' between them