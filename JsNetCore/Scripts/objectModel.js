var db = {
    tracks: {
        getById: function (id) {
            return JSON.parse(GetById("tracks", id));
        },

        insert: function (track) {
            Insert("tracks", track);
        },

        update: function (id, track) {
            Update(id, track);
        }
    }
};


















//var table = "{\"name\": \"Tracks\",\"displayName\": \"Треки\",\"description\": \"Список треков\",\"field\": \"Artist\"}";


/*
var model = {
    setModel: function (tableModel) {
        SetModel(tableModel);                   // C# method
    },

    getValueById: function (id) {
        return JSON.parse(GetValueById(id));    // C# method
    },

    addValue: function (value) {
        AddValue(track);                        // C# method
    },

    updateValue: function (id, value) {
        UpdateValue(id, value);                 // C# method
    }
}

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
