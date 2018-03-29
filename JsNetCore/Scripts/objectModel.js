var db = {
    tracks: {
        getById: function (id) {
            return JSON.parse(GetById(id));
        },

        insert: function (track) {
            Insert(track);
        },

        update: function (id, track) {
            Update(id, track);
        }
    }
};


/*
function getTrackById(trackId) {
    var path = getTrackPath(trackId);
    return getTrackBytes(path);
}

function getTrackPath(trackId) {
    var query = "SELECT ... WHERE id = trackId";
    return ExecSqlQuery(query);
}

function getTrackBytes(path) {
    return LoadFile(path);
}
*/