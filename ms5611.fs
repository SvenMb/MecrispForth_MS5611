$77 constant MS5611.ADDR

12 buffer: MS5611.params \ calibration data
6 buffer: MS5611.values \ rawdata

: MS5611-reset 
  MS5611.ADDR i2c-addr
  $1e >i2c
  0 i2c-xfer drop
;

: ms5611-i2c+ ( addr -- addr+1 )
 i2c> over c! 1+ 
;

: ms5611-rd ( addr n reg -- addr+n )
  MS5611.ADDR i2c-addr >i2c
  dup i2c-xfer drop
  0 do ms5611-i2c+ loop
;

: MS5611-calib
  
  MS5611.params
  6 0 do
		2 $a2 I 2 * + ms5611-rd
  loop
  drop
;

: ms5611-rawt
  MS5611.ADDR i2c-addr
  $58 >i2c 0 i2c-xfer drop
  10 ms
  MS5611.values 3 0 ms5611-rd drop
  MS5611.values dup c@ 16 lshift swap 1+ dup c@ 8 lshift swap 1+ c@ + +	
;

: ms5611-rawp
  MS5611.ADDR i2c-addr
  $48 >i2c 0 i2c-xfer drop
  10 ms
  MS5611.values 3 + 3 0 ms5611-rd drop
  MS5611.values 3 + dup c@ 16 lshift swap 1+ dup c@ 8 lshift swap 1+ c@ + +
;


: ms5611-temp

  ms5611-rawt
  MS5611.params dup 8 + c@ 8 lshift swap 9 + c@ + 256 * -
  MS5611.params dup 10 + c@ 8 lshift swap 11 + c@ + *
  8388608 / 2000 +
;

: ms5611-press
  ms5611-rawt
  MS5611.params dup 8 + c@ 8 lshift swap 9 + c@ + 256 * -
  dup
		
  MS5611.params dup 6 + c@ 8 lshift swap 7 + c@ + * 128 / s>d
  MS5611.params dup 2 + c@ 8 lshift swap 3 + c@ + 65536 um* d+
  dnegate

  rot
	
  MS5611.params dup 4 + c@ 8 lshift swap 5 + c@ + * 256 / s>d
  MS5611.params dup 0 + c@ 8 lshift swap 1 + c@ + 32768 um* d+
	
  ms5611-rawp s>d ud* 2097152 s>d d/ d+
  32768 um/mod swap drop
;

: MS5611-height

;


i2c-init
ms5611-reset
100 ms
ms5611-calib


: vario. 
  2200
  ms5611-press
  90000 -
  85 *
  100 /
  -

  .
  
;


