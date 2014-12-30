# Assembly code

Here there are some game functions that plays *SAD* files. I have commented
and clean the code to make them more readable. Now, it's easy to
understand the file format.

The code has been extracted from *Ni no kuni: Shikkoku no Madōshi*
Nintendo DS game.


## Function description
The functions are called in this order and with this purpose.

1. **playAudio**: Receive a structure with the path to the file to play.
  1. **initFile**: Create a new *audio_struct* and set some of its values.
  2. **getData** mode 1: Read the file header and copy to *audio_struct*.
  3. **copyChannelInfo2**: Initialize channel info fields of *audio_struct*.
  4. **setMode6**: Do something and then set *audio_struct* mode to 6.

2. **audio_mainLoop**:
  1. **getData** mode 6: Read the next block of audio data.
  2. **playNoLoop**: unknown, it has been set because there is no loop.
    1. 2x **procyon32**: decode samples for a channel in Procyon 32 KHz.
  3. 3x **getData** mode 6.
  4. **playNoLoop**.
    1. 2x **procyon32**.
  5. 3x **getData** mode 6.
  6. You are intelligent, you are seeing a pattern.
