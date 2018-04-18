var dbProvider = {
    insert: function (name, params) {
        return Insert(name, params);
    },
    updatebyrecid: function (name, recid, params) {
        return UpdateByRecid(name, recid, params);
    },
    findbyrecid: function (name, recid) {
        return FindByRecid(name, recid);
    },  
    findbyparams: function (name, params) {
        return FindByParams(name, params);
    }
}

// Controller and service
var devices = {
    save: function (params) {

        if (params.recid == null)
            return false;

        var deviceJson = dbProvider.findbyrecid("devices", params.recid);
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
        var dresult = dbProvider.insert("devices", params);

        return (uresult && dresult);
    },
    find: function (params) {
        if (params.recid == null)
            return false;

        return dbProvider.findbyrecid("devices", params.recid);
    },
    findbyuser: function (params) {
        return dbProvider.findbyparams("devices", params);
    }
}

// Controller and service
var histories = {
    save: function (params) {

        if (devices.find({ recid: params.deviceid }) == null || tracks.find({ recid: params.trackid }) == null)
            return false;

        if (params.recid == null)
            return false;

        if (!dbProvider.insert("histories", params))
            return false;

        var historyTemp = dbProvider.findbyrecid(params.recid);
        if (historyTemp != null) {
            var count = historyTemp["countsend"];
            historyTemp["countsend"] = countsend == null ? 0 : ++countsend;

            dbProvider.updatebyrecid("histories", historyTemp.recid, { countsend: count == null ? 0 : ++count });
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

        dbProvider.insert("histories", historyTemp);

        var ratingParam = { userid: params.userid, trackid: params.trackid };
        var rating = ratings.findbyparam(ratingParam);

        var dbresult = false;

        if (rating != null) {
            var ratingSum = rating["ratingsum"] + params.islisten;
            rating["lastlisten"] = params.lastlisten;
            rating["ratingsum"] = ratingSum;

            dbresult = ratings.updatebyrecid(rating);
        } else {
            dbresult = ratings.insert({
                userid: params.userid,
                trackid: params.trackid,
                lastlisten: params.lastlisten,
                islisten: params.islisten
            });
        }

        ratios.save(params.deviceid);

        return dbresult;
    },
    find: function (params) {
        if (params.recid == null)
            return false;

        return dbProvider.findbyrecid("histories", params.recid);
    }
}

// Controller and service
var tracks = {
    next: function (params) {
        return ExecSqlProcedure("getnexttrackid", { "i_deviceid": params.deviceid });
    },
    save: function (params) {
        if (params.recid == null)
            return false;

        return dbProvider.insert("tracks", params);
    },
    find: function (params) {
        var jObject = JSON.parse(dbProvider.findbyrecid("tracks", params.recid));

        tracks.fields.recid.value = jObject["recid"];
        tracks.fields.recname.value = jObject["recname"];
        tracks.fields.recdescription.value = jObject["recdescription"];
        tracks.fields.reccreated.value = jObject["reccreated"];
        tracks.fields.recupdated.value = jObject["recupdated"];
        tracks.fields.createdby.value = jObject["createdby"];
        tracks.fields.updatedby.value = jObject["updatedby"];
        tracks.fields.state.value = jObject["state"];

        tracks.fields.artist.value = jObject["artist"];
        tracks.fields.localdevicepathupload.value = jObject["localdevicepathupload"];
        tracks.fields.deviceid.value = jObject["deviceid"];
        tracks.fields.uploaduserid.value = jObject["uploaduserid"];
        tracks.fields.iscensorial.value = jObject["iscensorial"];
        tracks.fields.iscorrect.value = jObject["iscorrect"];
        tracks.fields.isfilledinfo.value = jObject["isfilledinfo"];
        tracks.fields.isexist.value = jObject["isexist"];
        tracks.fields.length.value = jObject["length"];
        tracks.fields.size.value = jObject["size"];

        tracks.fields.fileMp3.value = jObject["path"];

        return json;
    },
    findbydevice: function (params) {
        return dbProvider.findbyparams("tracks", params);
    },
    fields: {
        recid: { value: "" },
        recname: { value: "" },
        recdescription: { value: "" },
        reccreated: { value: "" },
        recupdated: { value: "" },
        createdby: { value: "" },
        updatedby: { value: "" },
        state: { value: "" },
        artist: { value: "" },
        localdevicepathupload: { value: "" },
        path: { value: "" },
        deviceid: { value: "" },
        uploaduserid: { value: "" },
        iscensorial: { value: "" },
        iscorrect: { value: "" },
        isfilledinfo: { value: "" },
        isexist: { value: "" },
        length: { value: "" },
        size: { value: "" },
        fileMp3: {
            value: "",
            getBlob: function () {
                return LoadFile(tracks.fields.fileMp3.value);
            }
        }
    },
    listen: function (params) {
        tracks.find(params);
        return tracks.fields.fileMp3.getBlob();
    }
}

// Service
var users = {
    save: function (params) {
        if (params == null)
            return false;

        return dbProvider.insert("users", params);
    },
    find: function (recid) {
        if (recid == null)
            return false;

        return dbProvider.findbyrecid("users", recid);
    }
}

// Service
var ratings = {
    save: function (params) {
        if (params == null)
            return false;

        return dbProvider.insert("ratings", params);
    },
    update: function (params) {
        if (params == null)
            return false;

        return dbProvider.updatebyrecid("ratings", params.recid, params);
    },
    findbyparams: function (params) {
        if (params == null)
            return false;

        return dbProvider.findbyparams("ratings", params);
    },
    findbyrecid: function (recid) {
        if (recid == null)
            return false;

        return dbProvider.findbyrecid("ratings", recid);
    }
}

// Service
var ratios = {
    save: function (params) {
        if (params == null)
            return false;

        return dbProvider.insert("ratios", params);
    },
    update: function (params) {
        return dbProvider.updatebyrecid("ratios", params);
    }
}

// Service
var downloadtrack = {
    save: function (params) {

    },
    findbytrack: function (params) {
        return dbProvider.findbyparams("downloadtrack", params);
    },
    findbydevice: function (params) {
        return dbProvider.findbyparams("downloadtrack", params);
    }
}
