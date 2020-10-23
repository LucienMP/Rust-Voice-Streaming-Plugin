@echo off
REM This will re-code the PCM file from a Steam Audio back to 8K WAV mono file

..\ffmpeg-4.3.1-2020-10-01-full_build\bin\ffmpeg -f s16le -ar 8k -ac 1 -i  test.pcm output.wav
pause