# Assembly code

Here there are some game functions that plays *SAD* files. I have commented
and clean the code to make them more readable. Now, it's easy to
understand the file format.

The code has been extracted from *Ni no kuni: Shikkoku no Madōshi*
Nintendo DS game.


## Function description
The functions are called in this order and with this purpose.

1. **playAudio**: Receive a structure with the path to the file to play.
2. **initFile**: Create a new *audio_struct* and set some of its values.
3. **getData** mode 1: Read the file header and copy to *audio_struct*.
4. **setMode6**: Do something and then set *audio_struct* mode to 6.
5. **getData** mode 6: Read the next block of audio data.
6. **noLoop**: unknown, it has been set because there is no loop.
7. **procyon32**: unknown, it has been set because it's Procyon 32 KHz.
8. **procyon32**.
9. **getData** mode 6.
10. **getData** mode 6.
11. **getData** mode 6.
12. **noLoop**.
13. **procyon32**.
14. **procyon32**.
15. **getData** mode 6.
16. **getData** mode 6.
17. **getData** mode 6.
18. **noLoop**.
19. **procyon32**.
20. **procyon32**.
21. ... You are intelligent, you are seeing a pattern.
