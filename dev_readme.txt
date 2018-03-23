First you need to listen clients from a port in server
After connecting any number of clients with different name to the server
Any user can receive a user list from server.
Not clients can play a game between each other. 

With invite button a client can invite other available client to play a game. 
After game starts clients guess numbers and whoever reaches 2 points win game and one point is added.
If one client surrenders or presses the disconnect button while it is in game, enemy player auto wins the game.
Server or client can close with using exit/disconnect button.

NOTE: instead of closing from X button in windows form may occour a problem related with ongoing threads. 
If you do such thing, you may need to close server from task manager. 
Good way of closing is, closing server first then closing clients. 
Server can not understand if client is disconnected or not if it is closed with windows form X button. So please close your client with disconnect button.


Communication achieved with using byte buffers with flags in it
buffer bit 0 - flag 
buffer bit 4 - message length
buffer bit 8 - message 

Additional messages added to future indexes but they depend on previous message length e.g buffer bit 8 + length index 

**Server send flags**

300-send user list
404 - name does not exist
442 - not exist 
445 - not busy so sendable
490 - rejected
500-disconnect tag
505 - name exists
545 - surrender info
525 - send accepted info
799 - send guess to other player
800 - check busy
801 - busy check
810 - round won send
811 - receiver lost round
812 - draw
813 - receiver lost game
816 - win game info
815 - win lose info 
900 - broadcast
600 - server closing





**Server receive flag**
200-incoming message from client
300-requested user list

440 - receive invite
480 - reject
500-disconnect tag
520-acccepted
540 - one surrendered
700 - not busy 
789 - player guessed
800 - busy check
810 - round won info
811 - round won/lost info
812 - draw 
813 - ?
814 - win lose info
816- win game info





**Client send flags*
200 - send message
300 - request user list
404 - name change
440 - invitation send
480 - reject invitaion
500 - exit
520 - accept invitation
540 - surrendered
540 - DisconnectLose
700-not busy
789 - guess sent
800- busy check 
810 - enemy win round
811 - me win round
812 - draw round
816 - me win game
814 - won game info

**Client receive flags**
300 - user list came from server
432 - Player in game
442 - Name not exist
445 - You are invited to play a game
490 - A player declined your request
500 - socket closing/disconnect
525 - A player accepted your request
545 - Enemy surrendered you won
600 - Server is closed
700 - Player is not busy/invitation send
799- Enemy guessed a number your turn
800 - Player busy
900 - incoming broadcast from server
801 * - Player is in busy
810 - You won this round
811 - Round lost
812 - Round draw
813 *- You lost the game
815 *- You lost the game
