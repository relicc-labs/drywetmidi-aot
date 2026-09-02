using System;
using System.IO;
using Melanchall.DryWetMidi.Core;

var midiData = new byte[]
{
    0x4D, 0x54, 0x68, 0x64, 0x00, 0x00, 0x00, 0x06,
    0x00, 0x00, 0x00, 0x01, 0x00, 0x60,
    0x4D, 0x54, 0x72, 0x6B, 0x00, 0x00, 0x00, 0x0F,
    0x00, 0xC0, 0x00,
    0x00, 0x90, 0x3C, 0x40,
    0x60, 0x80, 0x3C, 0x40,
    0x00, 0xFF, 0x2F, 0x00
};

using var stream = new MemoryStream(midiData, writable: false);
var midiFile = MidiFile.Read(stream);
using var converter = new MidiEventToBytesConverter { BytesFormat = BytesFormat.Device };
var convertedEvents = 0;
foreach (var chunk in midiFile.Chunks)
{
    if (chunk is not TrackChunk track)
        continue;

    foreach (var midiEvent in track.Events)
    {
        if (midiEvent is MetaEvent)
            continue;

        GC.KeepAlive(converter.Convert(midiEvent));
        convertedEvents++;
    }
}

if (convertedEvents != 3)
    throw new InvalidDataException($"Expected three channel events, but read {convertedEvents}.");
