var devices = {
    insert: function (params) {
        if (params.recId == null || params.recName == null)
            return false;

        return Insert("devices", params);
    },

    findbyrecid: function (params) {
        return FindById("devices", params.recId);
    }
}

var tracks = {

    next: function (params) {
        return ExecSqlProcedure("getnexttrackid", { "i_deviceid": params.deviceId });
    },

    insert: function (params) {
        if (params.recId == null)
            return false;

        return Insert("tracks", params);
    },

    findbyrecid: function (params) {
        return FindById("tracks", params.recId);
    }
}

var histories = {
    insert: function (params) {

        if (devices.findbyrecid({ recId: params.deviceId }) == null || tracks.findbyrecid({ recId: params.trackId }) == null)
            return false;

        if (params.recId == null)
            return false;

        return Insert("histories", params);
    },

    findbyrecid: function (params) {
        return FindById("histories", params.recId);
    }
}
