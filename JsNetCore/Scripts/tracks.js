function run(trackId) {

    db.tracks.insert({ recId: 1, artist: "track1", recName: "name1" });
    db.tracks.insert({ recId: 2, artist: "track2", recName: "name2" });
    db.tracks.insert({ recId: 3, artist: "track3", recName: "name3" });
    db.tracks.insert({ recId: 4, artist: "track4", recName: "name4" });
    db.tracks.insert({ recId: 5, artist: "track5", recName: "name5" });

    //db.tracks.update(1, { recId: 1, artist: "Imagine Dragons", recName: "Thunder" });

    return db.tracks.getById(trackId).artist;
}
