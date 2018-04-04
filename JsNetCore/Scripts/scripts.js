var devices = {
    insert: function (params) {
        if (params.recId == null || params.recName == null)
            return false;

        return ExecSqlProcedure("Insert", "devices", params);
    },

    findbyrecid: function (params) {
        return ExecSqlProcedure("findbyid", "devices", params.recId);
    }
}

var tracks = {

    next: function (params) {
        return ExecSqlProcedure("nexttrack", "tracks", params.deviceId);
    },

    insert: function (params) {
        if (params.recId == null)
            return false;

        return ExecSqlProcedure("Insert", "tracks", params);
    },

    findbyrecid: function (params) {
        return ExecSqlProcedure("findbyid", "tracks", params.recId);
    }
}

var histories = {
    insert: function (params) {
        //if (devices.findbyrecid(params.deviceId) == null || tracks.findbyrecid(params.trackId) == null)
            //return false;

        if (params.recId == null)
            return false;

        return ExecSqlProcedure("Insert", "histories", params);
        /*if (historyJson != null) {
            var historyTemp = JSON.parse(historyJson);
            historyTemp.countsend = historyTemp.countsend == null ? 0 : ++historyTemp.countsend;

            return ExecSqlProcedure("Insert", "histories", historyTemp);
        }*/
    },

    findbyrecid: function (params) {
        return ExecSqlProcedure("findbyid", "histories", params.recId);
    }
}
