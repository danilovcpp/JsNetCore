var devices = {
    save: function (params) {

        if (params.recid == null)
            return false;

        var deviceJson = devices.find("devices", params);
        if (deviceJson != "")
            return false;

        if (params.recname == null)
            params.recname = "New unknow device";

        var userJson = users.find(params.recid);
        var user = null;

        if (userJson != "")
            user = JSON.parse(userJson);

        if (user == null) {
            user = { recid: params.recid, recname: params.recname };
        } else {
            user["recname"] = params.recname;
        }

        var uresult = users.save(user);
        params.userid = user.recid;
        var dresult = Insert("devices", params);

        return (uresult && dresult);
    },
    find: function (params) {
        if (params.recid == null)
            return false;

        return FindByRecid("devices", params.recid);
    }
}

var histories = {
    save: function (params) {

        if (devices.find({ recid: params.deviceid }) == "" || tracks.find({ recid: params.trackid }) == "")
            return false;

        if (params.recid == null)
            return false;

        var historyTemp = null;
        var historyTempJson = histories.find(params);

        if (historyTempJson != "")
            historyTemp = JSON.parse(historyTempJson);

        if (historyTemp != null) {
            var count = historyTemp["countsend"];
            UpdateByRecid("histories", historyTemp.recid, { countsend: count == null ? 0 : ++count });
        } else {
            historyTemp = {
                recid: params.recid,
                trackid: params.trackid,
                deviceid: params.deviceid,
                countsend: 1,
                islisten: params.islisten,
                lastlisten: params.lastlisten
            }
        }

        Insert("histories", historyTemp);

        var ratingJson = ratings.findbyparams({ userid: params.userid, trackid: params.trackid });
        var rating = null;
        var dbresult = false;

        if (ratingJson != "") {
            rating = JSON.parse(ratingJson);
        }

        if (rating != null) {
            dbresult = ratings.update({ recid: rating[0].recid, lastlisten: params.lastlisten, ratingsum: Number(rating[0].ratingsum) + Number(params.islisten) });
        } else {
            dbresult = ratings.save({
                userid: params.userid,
                trackid: params.trackid,
                lastlisten: params.lastlisten,
                ratingsum: params.islisten
            });
        }

        //TODO: ratio update

        return dbresult;
    },
    find: function (params) {
        if (params.recid == null)
            return false;

        return FindByRecid("histories", params.recid);
    }
}

var tracks = {
    next: function (params) {
        return ExecSqlProcedure("getnexttrackid", { "i_deviceid": params.deviceid });
    },
    save: function (params) {
        if (params == null)
            return false;

        return Insert("tracks", params);
    },
    find: function (params) {
        if (params.recid == null)
            return "";

        var trackJson = FindByRecid("tracks", params.recid);
        if (trackJson == "")
            return "";

        tracks.fields = JSON.parse(trackJson);
        tracks.fileMp3.value = tracks.fields.path;
        
        return trackJson;
    },
    findbydevice: function (params) {
        return FindByParams("tracks", params);
    },
    fields: {},
    fileMp3: {
        value: "",
        getBlob: function () {
            return LoadFile(tracks.fileMp3.value);
        }
    },
    listen: function (params) {
        tracks.find(params);
        return tracks.fileMp3.getBlob();
    }
}

var users = {
    save: function (params) {
        if (params == null)
            return false;

        return Insert("users", params);
    },
    find: function (recid) {
        if (recid == null)
            return false;

        return FindByRecid("users", recid);
    }
}

var ratings = {
    save: function (params) {
        if (params == null)
            return false;

        return Insert("ratings", params);
    },
    update: function (params) {
        if (params == null)
            return false;

        return UpdateByRecid("ratings", params.recid, params);
    },
    findbyparams: function (params) {
        if (params == null)
            return false;

        return FindByParams("ratings", params);
    },
    find: function (recid) {
        if (recid == null)
            return false;

        return FindByRecid("ratings", recid);
    }
}
