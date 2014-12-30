;; Code extracted from game. Some instruction has been modified for good reading
;; Copyright (C) Nintendo and others
;; Use only for learning purpouse
;;
;; Function: procyon32
;; Arguments:
;;  + R0: audio_struct
;;  + R1: output pointer
;;  + R2: input pointer: audio data block
;;  + R3: 0x200
;; Stack variables:
;;  + blockSize= -0x48
;;  + scale= -0x44
;;  + smpBlockNum= -0x40  Sample Block Num
;;  + input= -0x3C
;;  + readSize= -0x38
;;  + chunkSize= -0x34
;;  + histOffset= -0x30
;;  + maxValue= -0x2C
;;
;; Encoded data has the following format:
;; * Chunks (size 0x10)
;;  + Chunk for channel 0
;;  + Chunk for channel 1
;;  + Chunk for channel 0
;;  + Chunk for channel 1
;;  ...
;; Each chunk has a header and encoded samples
;;  + Header: 8 bits (4 bits for coeff idx and 4 bits for scale)
;;  + 30 encoded samples: each one is 4 bits.

  ; Saves registers
  STMFD   SP!, {R3-R11,LR}
  SUB     SP, SP, #0x20

  ; Store chunk size
  LDRH    R4, [R0,#0x9E]            ; Read chunk size halfwowrd from file 0x3A
  STR     R4, [SP,#0x48+chunkSize]  ; ... and write it

  ; Get the size of a block of chunks (a chunk per channel)
  LDRB    R4, [R0,#0x23]            ; Get number of channels
  LDR     R5, [SP,#0x48+chunkSize]  ; Get chunk size
  MUL     R4, R5, R4                ; Get the length of all the chunks together
  STR     R4, [SP,#0x48+blockSize]  ; ... write it

  ; Get offset to historial values
  MOV     R5, #0x2C                 ; Info block size
  ADD     R6, R0, #0x220            ; Get base dir 0x220
  LDRB    R7, [R0,#0x28]            ; Get current channel number
  MLA     R0, R7, R5, R6            ; Get offset
  STR     R0, [SP,#0x48+histOffset] ; ... and write it

  ; Go to first chunk for this channel in this block of data
  LDR     R0, [SP,#0x48+chunkSize]  ; Get the size of the chunk
  MLA     R2, R7, R0, R2            ; ... and go to our chunk

  ; Set data read to 0
  MOV     R0, #0                    ; 0 bytes read
  STR     R0, [SP,#0x48+readSize]   ; ... write it

  ; Check that block size is not 0
  CMP     R3, #0                    ; Skip in that case
  BLE     quit                      ; ...
smpBlockNum
; For each chunk
decode_chunk:
  ; Set output pointer and store current input pointer
  MOV     R8, R1
  STR     R2, [SP,#0x48+input]

  ; Parse header of the chunk
  LDRB    R0, [R2,#0xF]             ; Get flag byte
  EOR     R0, R0, #0x80             ; ... decode it
  AND     R0, R0, #0xFF             ; ...

  ; ... Get coefficient index
  MOV     R4, R0,ASR#4              ; Second half-byte: bits 4-7
  CMP     R4, #4                    ; ... and maximum 5 values
  MOVGT   R4, #0                    ; ...

  ; ... Get scale value
  AND     R0, R0, #0xF              ; First half-byte: bits 0-3
  RSB     R0, R0, #0xC              ; ... substracting 0xC
  STR     R0, [SP,#0x48+scale]      ; ... and write it

  ; ... Get coefficients
  LDR     R0, =0x020682AC           ; Coefficient table
  MOV     R4, R4,LSL#1              ; ... 0xFFFF, 0xFFC3, 0x338C, 0x369D, 0x3B85
  LDRH    R4, [R0,R4]               ; ... Read both coefficients
  MVN     R0, R4                    ; Decode them
  MOV     R0, R0,LSL#16             ; ... make sure it's only two bytes
  MOV     R0, R0,LSR#16             ; ...
  MOV     R4, R0,ASR#8              ; Split them: second coefficient
  MOV     R4, R4,LSL#24             ; ... it's the byte (bit 8-15)
  MOV     R11, R4,ASR#24            ; ... and only a byte
  MOV     R0, R0,LSL#24             ; Split them: first coefficient
  MOV     R5, R0,ASR#24             ; ... since it's the first byte, just AND

  ; Set to 0 the samples blocks read
  MOV     R0, #0                    ; Each sample block
  STR     R0, [SP,#0x48+smpBlockNum]; ... it's 4 bytes (a word)

  ; Read historical values
  LDR     R0, [SP,#0x48+histOffset] ; Get offset
  LDR     R7, [R0]                  ; First value
  LDR     R6, [R0,#4]               ; Second value

  ; Calculate the max value for a signed 16 bits value
  MOV     R0, #0x8000               ; It's ...
  RSB     R0, R0, #0                ; ... the number ...
  MOV     R4, R0,LSR#17             ; ... mmm, uh? ...
  STR     R4, [SP,#0x48+maxValue]   ; ... 0x7FFF

decode_sample_block:
  ; Set the number of samples decoded to 0
  MOV     LR, #0

  ; Read new block of samples
  LDR     R4, [SP,#0x48+input]      ; Get the input pointer
  LDR     R9, [R4],#4               ; ... read a block of samples (4 bytes)
  LDR     R4, =0x80808080           ; ... decoded them
  EOR     R4, R9, R4                ; ...
  STR     R4, [SP,#0x48+input]      ; ... and store the updated pointer

  ; If last sample block, there are only 6 samples, because of the 8-bit header
  LDR     R9, [SP,#0x48+smpBlockNum]; Get sample block index
  CMP     R9, #3                    ; If it's the last block
  MOVEQ   R9, #6                    ; ... there are only 6 samples (24 bits)
  MOVNE   R9, #8                    ; else, the full word it's for samples

decode_sample:
  ; Get the encoded sample byte shifted to the left by 8
  MOV     R10, R4,LSL#28            ; Gets only a byte shifted by 8
  MOV     R10, R10,LSR#16           ; ...
  MOV     R10, R10,LSL#16           ; The 16-bit value is signed, so set it
  MOV     R10, R10,ASR#16           ; ... with ASR the bit sign it's keept

  ; Scale the value
  LDR     R12, [SP,#0x48+scale]     ; Get the scale value and scale it.
  MOV     R12, R10,ASR R12          ; ... At the end we multiply by 256 / scale

  ; Operate with the historical values
  MUL     R10, R6, R11              ; hist = hist1 * coeff1 + hist0 * coeff0
  MLA     R10, R7, R5, R10          ; ...
  MOV     R6, R10,ASR#5             ; Adds 31 if negative. With ASR, the bit
  ADD     R6, R10, R6,LSR#26        ; ... sign is copied, so ASR 5 = 0x1F = 31

  ; Add the sample with the historical
  MOV     R6, R6,ASR#6              ; sample = (sample * 64) + (hist / 64)
  ADD     R10, R6, R12,LSL#6        ; ...

  ; Update historical values
  MOV     R6, R7                    ; hist1 = hist0
  MOV     R7, R10                   ; hist0 = sample

  ; Get the final sample
  MOV     R12, R10,ASR#5            ; Adds 31 if negative as above
  ADD     R10, R10, R12,LSR#26      ; ...
  MOV     R10, R10,ASR#6            ; sample /= 64
  ADD     R10, R10, #0x20           ; sample += 32

  ; Check that it's in the limits of a signed 16 bits value
  CMP     R10, R0,LSR#17            ; Compare sample with 0x7FFF
  LDRGT   R10, [SP,#0x48+maxValue]  ; If greater, set it
  BGT     decode_sample_cond        ; ...
  CMP     R10, R0                   ; Compare sample with 0xFFFF8000
  MOVLT   R10, R0                   ; if it's less, set it

decode_sample_cond:
  ; Store the decoded sample and increment pointer
  STRH    R10, [R8],#2

  ; Go for next encoded sample
  MOV     R4, R4,LSR#4

  ; If we have read all of them, go for another block
  ADD     LR, LR, #1                    ; Increment number of samples decoded
  CMP     LR, R9                        ; ... and check with the total
  BLT     decode_sample                 ; ... jump!

decode_sample_block_cond:
  ; Check that the full chunk have been decoded
  LDR     R4, [SP,#0x48+smpBlockNum]    ; Increment decoded samples block number
  ADD     R4, R4, #1                    ; ...
  STR     R4, [SP,#0x48+smpBlockNum]    ; ...
  CMP     R4, #4                        ; ... and if it's less than 4
  BLT     decode_sample_block           ; ... (the 0x10 block size), jump!

decode_chunk_cond:
  ; Store the current historical values
  LDR     R0, [SP,#0x48+histOffset]     ; Get historical offset
  STR     R7, [R0]                      ; ... and store historical value 0
  STR     R6, [R0,#4]                   ; ... and store historical value 1

  ; Increment the bytes read with the chunk size
  LDR     R4, [SP,#0x48+readSize]       ; Get the number of bytes read now
  LDR     R0, [SP,#0x48+chunkSize]      ; ... and adds the size of this chunk
  ADD     R0, R4, R0                    ; ...
  STR     R0, [SP,#0x48+readSize]       ; ... and store it

  ; Increment the output pointer by 0x3C (0x1E samples of 2 bytes)
  ADD     R1, R1, #0x3C

  ; Checks if we can read more blocks
  LDR     R0, [SP,#0x48+readSize]       ; Bytes read at the moment
  CMP     R0, R3                        ; Check with buffer size
  LDR     R0, [SP,#0x48+blockSize]      ; ... if positive, go to next block
  ADD     R2, R2, R0                    ; ... of channels headers
  BLT     decode_chunk                  ; ... and jump!

quit:
  MOV     R0, #0                        ; Everything goes fine :)
  ADD     SP, SP, #0x20                 ; Returns variables
  LDMFD   SP!, {R3-R11,PC}              ; ...
