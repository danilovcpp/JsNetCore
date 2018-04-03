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
    insert: function (params) {
        if (params.recId == null)
            return false;

        return ExecSqlProcedure("Insert", "tracks", params);
    },

    findbyrecid: function (params) {
        return ExecSqlProcedure("findbyid", "tracks", params.recId);
    }
}
