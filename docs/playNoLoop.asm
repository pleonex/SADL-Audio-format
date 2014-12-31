;; Code extracted from game. Some instruction has been modified for good reading
;; Copyright (C) Nintendo and others
;; Use only for learning purpouse
;;
;; Function: playNoLoop
;; Arguments:
;;  + R0: audio_struct
;; Returns:
;;  + R0: 0

  ; Save registers
  STMFD   SP!, {R4-R10,LR}
  MOV     R4, R0

  ; The useful constant
  MOV     R5, #0

  ; Get the current data offset
  LDR     R6, [R4,#0x54]    ; Bytes processed (file offset)
  LDR     R0, [R4,#0x5C]    ; File size
  SUBS    R7, R0, R6        ; Get remaining data size
  BEQ     after_process     ; If 0, ok, no need to process more data

  ; Get bytes to read per channel
  LDR     R0, [R4,#0x1F0]   ; Number of samples to decode
  LDR     R1, [R4,#0x1DC]   ; Number of samples per chunk
  BL      sdk_fastDiv       ; ... Gets number of chunks to decode
  LDRH    R1, [R4,#0x9E]    ; Chunk size
  MUL     R1, R0, R1        ; ... Gets number of bytes to read

  ; Get total bytes to read
  LDRB    R2, [R4,#0x23]    ; Get number of channels
  MUL     R0, R2, R1        ; Mutiplies bytes to read X channels
  CMP     R0, R7            ; Check if we have less data than maximum
  MOVLE   R7, R0            ; Then set it

  ; Reads a data block
  MOV     R0, R4            ; audio_struct
  LDR     R1, [R4,#0x1D8]   ; Data block output pointer
  MOV     R2, R7            ; Data block length
  MOV     R3, R6            ; File offset (bytes proccessed)
  LDR     R8, [R4,#0x200]   ; Get function
  BLX     R8                ; ... call it
  CMP     R0, #0            ; If no error, continue
  BNE     thereAreDataBlock ; ...

  LDR     R0, [R4]
  MOV     R1, #0xC
  MOV     R7, #0
  MOV     R2, R7
  LDR     R3, [R4,#0x1F8]
  LDR     R6, [R4,#0x1F4]
  BLX     R6

  LDRB    R0, [R4,#0x23]
  CMP     R0, #0
  BLE     loc_20553B4

  loc_2055390
  ADD     R0, R4, R7,LSL#2
  LDR     R1, [R0,#0x1CC]
  LDR     R2, [R4,#0x1EC]
  MOV     R0, R5
  BL      sub_2009B84

  LDRB    R0, [R4,#0x23]
  ADD     R7, R7, #1
  CMP     R7, R0
  BLT     loc_2055390

  loc_20553B4
  MOV     R0, 0xFFFFFF48
  MOV     R1, #0
  MOV     R2, R1
  BL      sub_204F2A8
  B       quit

thereAreDataBlock:
  ; Get the byte to read per channel
  MOV     R0, R7            ; Bytes to decode
  LDRB    R5, [R4,#0x23]    ; Number of channels
  MOV     R1, R5            ; ...
  BL      sdk_fastDiv       ; Bytes to read for each channel

  ;  Get number of chunks
  MOV     R8, R0            ; Bytes for channel
  LDRH    R1, [R4,#0x9E]    ; Chunk size
  BL      sdk_fastDiv2      ; Number of chunks in the channel

  ; Get total decoded samples size
  LDR     R1, [R4,#0x1DC]   ; Get number of samples per channel
  MUL     R0, R1, R0        ; Num chunks * samples per chunk
  MOV     R9, R0,LSL#1      ; ... each decode sample is 16-bit

  ; Channel index
  MOV     R10, #0

  ; If there is no channel, skip
  CMP     R5, #0
  BLE     update_offset
  MOV     R5, R10

decode_channel:
  STRB    R10, [R4,#0x28]       ; Set current channel decoding

  ; Get the pointer to the decoded samples
  ADD     R0, R4, R10,LSL#2     ; It's 0x1CC + 4 * i
  LDR     R6, [R0,#0x1CC]       ; ...
  MOV     R1, R6                ; ...

  ; Decode current channel block
  MOV     R0, R4                ; audio_struct
  LDR     R2, [R4,#0x1D8]       ; Encoded data samples
  MOV     R3, R8                ; Chunk block size
  LDR     R12, [R4,#0x204]      ; Decoding function
  BLX     R12                   ; ... call it

  ; Check if we have fulfill the output samples buffer
  LDR     R2, [R4,#0x1EC]       ; Total decoded samples size
  CMP     R9, R2                ; Current bytes read
  BEQ     cond_decode_channel   ; Are equals

  MOV     R0, R5
  ADD     R1, R6, R9
  SUB     R2, R2, R9
  BL      sub_2009B84

cond_decode_channel:
  ADD     R10, R10, #1          ; Increment channel index
  LDRB    R0, [R4,#0x23]        ; Check if all channels have been processed
  CMP     R10, R0               ; ...
  BLT     decode_channel        ; Nop

update_offset:
  LDR     R0, [R4,#0x54]        ; Update the data block offset
  ADD     R0, R0, R7            ; ...
  STR     R0, [R4,#0x54]        ; ...
  B       quit                  ; Quit!

after_process:
  MOV     R6, R5
  LDRB    R0, [R4,#0x23]
  CMP     R0, #0
  BLE     loc_2055494

  loc_2055470
  ADD     R0, R4, R6,LSL#2
  LDR     R1, [R0,#0x1CC]
  LDR     R2, [R4,#0x1EC]
  MOV     R0, R5
  BL      sub_2009B84

  LDRB    R0, [R4,#0x23]
  ADD     R6, R6, #1
  CMP     R6, R0
  BLT     loc_2055470

  loc_2055494
  LDRSB   R0, [R4,#0xF]
  CMP     R0, #1
  MOVNE   R0, #1
  STRBNE  R0, [R4,#0xF]
  BNE     quit

  STRB    R5, [R4,#0xA]
  STRB    R5, [R4,#0x13]
  MOV     R0, R4
  BL      sub_2055A78

  LDR     R0, [R4]
  MOV     R1, #8
  MOV     R2, R5
  LDR     R3, [R4,#0x1F8]
  LDR     R4, [R4,#0x1F4]
  BLX     R4

quit:
  MOV     R0, #0
  LDMFD   SP!, {R4-R10,PC}
