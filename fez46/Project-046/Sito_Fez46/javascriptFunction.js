function myMap() {
    var mapCanvas = document.getElementById("map");
    var mapOptions = {
        center: new google.maps.LatLng(45.0704900, 7.6868200), zoom: 10
    };
    var map = new google.maps.Map(mapCanvas, mapOptions);
}

//configurazioni varie 
var conf = [];
var AllData=[];
//sensori possibili
var sensorPossible = ["temperature", "moisture", "lightsense", "humidity", "position", "water", "co2", "fire", "flame", "other"]
var sensorPossibleDiv = ["sensorsDivT", "sensorsDivM", "sensorsDivL", "sensorsDivH", "sensorsDivP", "sensorsDivW", "sensorsDivco2", "sensorsDivF", "sensorsDivFl", "sensorsDivOther"]
var fezID;

function getConffromAWS() {
    AWS.config.update(
        {
            //accessKeyId: "AKIAISZJ6CTVUAKSKWSA",
            accessKeyId: "AKIAIVW3V6JTLWTSG3AQ",//idilio
            //secretAccessKey: "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",
            secretAccessKey: "o6i1i4XlDCL+ZwtHm240BODJfLO8sTw3Bj4N3E0z",//idilio
            region: "us-east-1"
            // region: "eu-west-1",//idilio      
        });
    var s3 = new AWS.S3();
    var p = {
        //Bucket: "my-iot-data2018",
        Bucket: "polito.plcs", //idilio
        Prefix : "configurations/FEZ"
        //Key: "configuration.json"
    };

    s3.listObjects(p, function (err, data) {
        var list = [];
        var listindex = 0;
        var tutteLePromesse = [];
        //fare ciclo per prendere tutti i file conf //per ogni fez metto un marker
        for (i = 0; i < data.Contents.length; i++) {
            //per nostre prove
        //    var n = data.Contents[i].Key;
            // console.log(n);
           // n = n.split('/');

           // if (n[0] == "measurements" || n[1] == "") { continue; }
            list[listindex] = data.Contents[i].Key;
            var p = {
                //Bucket: "my-iot-data2018",
                Bucket: "polito.plcs",//idilio
                Key: list[listindex]
            };
            
            tutteLePromesse[listindex] = s3.getObject(p).promise();
            listindex++;
        }
       // console.log(list);

        Promise.all(tutteLePromesse).then(function (values) {
            var rightConf = [];


            for (j = 0; j < values.length; j++) {
                
                o = JSON.parse(values[j].Body.toString("utf-8"));
                
                if (rightConf[o.id] == null) {
                    var vectC = {
                        last: null,
                        conf: null
                    };
                    vectC.last = values[j].LastModified;
                    vectC.conf = o;
                    rightConf[o.id] = vectC;

                }

                if (rightConf[o.id].last < values[j].LastModified) {
                    rightConf[o.id].last = values[j].LastModified;
                    rightConf[o.id].conf = o;
                }
                //    saveConfiguration(o);
            }



            for (var l in rightConf) {

                saveConfiguration(rightConf[l].conf);
            }
            // o = JSON.parse(rightConf[j].conf.toString("utf-8"));

            initMap();
        });
    });
}

// Initialize and add the map
function initMap() {
    //getConffromAWS();
    //while(conf.length==0) continue;
    //creo una volta la mappa
    var map = new google.maps.Map(document.getElementById('map'), { zoom: 12, center: new google.maps.LatLng(45.0704902, 7.6868204) });
    for (var i = 0; i < conf.length; ++i) {
        var uluru = { lat: conf[i].latitude, lng: conf[i].longitude };
        //var image = 'C://Users//Andre//Desktop//ESAMI//Sito_guido//fez36.png';
        contentInfoWindows = '<div><strong>' + conf[i].id + '</strong><br>' +
            'Location: ' + conf[i].location + '<br>' +
            'Sensors: ' + findSensorsString(conf[i].sensors) + '</div>' + '<button type="button" class="myButton" value="Choose" onclick="disableCheckBox()"> <i class="" style="text-align: center" role="presentation"></i>Choose this Fez</button>';

       // if (conf.length == 0) { getConffromAWS(); }
        var marker = new google.maps.Marker({
            position: uluru,
            map: map,
            animation: google.maps.Animation.DROP,
            title: conf[i].id,
            buborek: contentInfoWindows
        }); //icon: image
        var infowindow = new google.maps.InfoWindow({});
        /*  infowindow.setContent('<div><strong>' + conf[i].id + '</strong><br>' +
              'Location: ' + conf[i].location + '<br>' +
              'Sensors: ' + findSensorsString(conf[i].sensors) + '</div>' + '<button type="button" class="myButton" value="Chose" onclick="disableCheckBox()"> <i class="" style="text-align: center" role="presentation"></i>Chose this Fez</button>');
  */
        marker.addListener('click', function () {
            infowindow.setContent(this.buborek);
            fezID = this.title;
            infowindow.open(this.get('map'), this);

        });
    }
}


function getFezID() {
    return fezID;
}

function disableCheckBox() {
    var divS = document.getElementById("sensors");
    var html = "";
    for (i = 0; i < conf.length; i++) {
        if (conf[i].id == fezID) {
            for (j = 0; j < conf[i].sensors.length; j++) {
                html = html + "<label class=\"switch\"><input type=\"checkbox\" id=\"" + conf[i].sensors[j].type.toString() + "\" value=\"false\" onchange=\"showDiv('" + conf[i].sensors[j].name.toString() + "','" + conf[i].sensors[j].type.toString() + "','charts')\"> <span class=\"slider round\"></span></label><label for=\"temp\">&emsp;" + conf[i].sensors[j].name.toString() + "</label><br><br>";
            }
        }
    }
    divS.innerHTML = html;
    /* deleteCharts();
     var sensorActive;
     for (var i = 0; i < conf.length; ++i) {
         if (conf[i].id == fezID) {
             sensorActive = conf[i].sensors;
             break;
         }
     }
 
     for (i = 0; i < sensorPossible.length; i++) {
         var checkBox = document.getElementById(sensorPossible[i]);
         var text = document.getElementById(sensorPossibleDiv[i]);
         if (checkBox == null) continue;
         if (checkBox.checked == true)
             text.style.display = "none";
         if (checkBox == null) continue;
         checkBox.checked = false;
         var flag = false;
         for (j = 0; j < sensorActive.length; j++) {
             if (sensorPossible[i] == sensorActive[j].type) {
                 flag = true;
                 break;
             }
         }
         if (flag == false) {
             checkBox.disabled = true;
             checkBox.parentNode.style.opacity = 0.5;
         }
         else {
             checkBox.disabled = false;
             checkBox.parentNode.style.opacity = 1;
         }
     }*/
}

function findSensorsString(sensors) {
    var str = "";
    for (i = 0; i < sensors.length; i++) {
        str = str.concat(" " + sensors[i].name);
    }
    return str;
}

function saveConfiguration(data) {
    //  alert('data');
    var flag = false;
    for (var i = 0; i < conf.length; ++i) {
        if (conf[i].id == data.id) {
            if (conf[i].location == data.location) {
                flag = true;
                break;
            } else conf.splice(i, 1);
        }
    }
    if (flag == false)
        conf[conf.length] = data;
}

function showDiv(name, strID, chartID) {
    var x = document.getElementById("map");
    if (x.style.display == 'block') {
        x.style.display = 'none';
    }
    var y = document.getElementById("info");
    if (y.style.display == 'block') {
        y.style.display = 'none';
    }
    var z = document.getElementById("data");
    if (z.style.display == 'block') {
        z.style.display = 'none';
    }
    createCharts();
    // Get the checkbox
    var checkBox = document.getElementById(strID);
    var canvasID = "myChart" + name;
    var divID = "div" + name;

    var html = "<div style=\"display:none;\" id=\"" + divID + "\" class=\"demo-graphs mdl-shadow--2dp mdl-cell mdl-cell--12-col\">" + name + "<canvas id=\"" + canvasID + "\"width=\"800\" height=\"400\"></canvas></div>";
    var newNode = document.createElement('div');
    newNode.innerHTML = html;
    document.getElementById(chartID).appendChild(newNode);
    
    if (checkBox.checked == true) {
        document.getElementById(divID).style.display = "block";
        GraphCharts(name, canvasID);
    }
    else {
        document.getElementById(divID).style.display = "none";
    }
  
    // Get the output text
    /*var text = document.getElementById(str);
    // If the checkbox is checked, display the output text
    if (checkBox.checked == true) {
        text.style.display = "block";
        GraphCharts(strID, chartID, fezID);
    } else {
        text.style.display = "none";
    }*/
}

function showMap(str) {
    var x = document.getElementById("info");
    if (x.style.display == 'block') {
        x.style.display = 'none';
    }
    var home = document.getElementById("home");
    if (home.style.display == 'block') {
        home.style.display = 'none';
    }
    var z = document.getElementById("data");
    if (z.style.display == 'block') {
        z.style.display = 'none';
    }
    deleteCharts();
    var x = document.getElementById(str);
    if (x.style.display == 'none') {
        x.style.display = 'block';
        //initMap();
        getConffromAWS();
    }
    else {       
        x.style.display = 'none';
        home.style.display = 'block';
        createCharts();
    }
}

function showNav(str) {
    var colle
    var x = document.getElementById("map");
    if (x.style.display == 'block') {
        // x.style.display = 'none';
        createCharts();
        //return;
    }
    var home = document.getElementById("home");
    if (home.style.display == 'block') {
        home.style.display = 'none';
    }
    var x = document.getElementById(str);
    if (x.style.display == 'none') {
        x.style.display = 'block';
    }
    else {
        x.style.display = 'none';
    }
}

function showInfo(str) {
    var x = document.getElementById("map");
    if (x.style.display == 'block') {
        x.style.display = 'none';
    }
    var home = document.getElementById("home");
    if (home.style.display == 'block') {
        home.style.display = 'none';
    }
    deleteCharts();
    var x = document.getElementById(str);
    if (x.style.display == 'none') {
        x.style.display = 'block';
    }
    else {
        x.style.display = 'none';
        createCharts();
    }
}

function showData(str) {
    var selectedSensor;
    var i = 0;
    conf.forEach(function(element){
        
        if(element.id == fezID)
        selectedSensor = i;
        i++;

    });
    var home = document.getElementById("home");
    if (home.style.display == 'block') {
        home.style.display = 'none';
    }
    var x = document.getElementById("map");
    if (x.style.display == 'block') {
        x.style.display = 'none';
    }
    var x = document.getElementById("info");
    if (x.style.display == 'block') {
        x.style.display = 'none'; 
    }
    deleteCharts();
    var x = document.getElementById(str);
    x.style.textAlign = "center";
    html = "";
    x.innerHTML=html;
    var html="<br> <h3 align='left'>"+fezID+"</h3><br>" ;
    html += "<table align='center'> <tr> <th>Sensor</th> <th> Value</th><th>Timestamp</th> </tr> " ;

 //   if (x.style.display == 'none') {
        x.style.display = 'block';
/*        for (var i = 0; i < AllData.length; ++i) {
            html=html+AllData[i].y+"<br>";
        }
        x.innerHTML+=html;
        */
       AWS.config.update(
        {
            //accessKeyId: "AKIAISZJ6CTVUAKSKWSA",
            accessKeyId: "AKIAIVW3V6JTLWTSG3AQ",//idilio
            //secretAccessKey: "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",
            secretAccessKey: "o6i1i4XlDCL+ZwtHm240BODJfLO8sTw3Bj4N3E0z",//idilio
            region: "us-east-1"
            //region: "eu-west-1"//idilio
        });

    var s3 = new AWS.S3();
    var params1 = {
        //Bucket: "my-iot-data2018",
        Bucket: "polito.plcs", //idilio
        Prefix: "measurements/" + fezID
    };
    s3.listObjects(params1, function (err, data) {
        allMisure = [];
        data.Contents.forEach(function(element){
            allMisure.push(s3.getObject({Bucket:"polito.plcs", Key: element.Key}).promise());
        })
        Promise.all(allMisure).then(function (values) {
        values.forEach(function(element){
            var pack = JSON.parse(element.Body.toString("utf-8"));
            pack.measurements.forEach(function(element){
                 var s = id2type(element.sensor_id,conf[selectedSensor]);
                 html+= "<tr><th align='left'>"+s+"</th><th>"+element.value+"</th><th>"+element.iso_timestamp+"</th></tr>"
                //html = html +" "+ s +" "+ element.value+" "+element.iso_timestamp+"<br>";
            });

        })
           html+="</table>";
            x.innerHTML+=html;  
        });
    });
   // }  
   /* else {
        html = "";
        x.innerHTML=html;
        x.style.display = 'none';      
        createCharts();
    }
    */
}

function deleteCharts() {
    var x = document.getElementById("charts");
    x.style.display = 'none';
}

function createCharts() {
    var x = document.getElementById("charts");
    x.style.display = 'block';
}

function pop(strID) {
    alert(strID);
}


function GraphCharts(sensor, chart) {

    AWS.config.update(
        {
            //accessKeyId: "AKIAISZJ6CTVUAKSKWSA",
            accessKeyId: "AKIAIVW3V6JTLWTSG3AQ",//idilio
            //secretAccessKey: "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",
            secretAccessKey: "o6i1i4XlDCL+ZwtHm240BODJfLO8sTw3Bj4N3E0z",//idilio
            region: "us-east-1"
            //region: "eu-west-1"//idilio
        });

    var s3 = new AWS.S3();
    var params1 = {
        //Bucket: "my-iot-data2018",
        Bucket: "polito.plcs", //idilio
        Prefix: "measurements/" + fezID
    };

    s3.listObjects(params1, function (err, data) {
        var list = [];
        //console.log(data);
        // console.log(err);
        var allPromise = [];
        data.Contents.reverse();
        var listindex = 0;
        // for (i = 0; (i < data.Contents.length && i < 10); i++) {
        for (i = 0; ( i < data.Contents.length); i++) {
            var n = data.Contents[i].Key
            //console.log(n);
            n = n.split('/');

            if (n[0] == "configurations") continue;
            var nomeM = n[1];
            var nomeM = n[1].split('_');
            var nome = nomeM[0] + '_' + nomeM[1];
            if (nome != fezID) continue;
            list[listindex] = data.Contents[i].Key;
            var p = {
                //Bucket: "my-iot-data2018",
                Bucket: "polito.plcs", //idilio
                Key: list[listindex]
            };

            allPromise[listindex++] = s3.getObject(p).promise();
        }

        var config_sensor = [];
        for (i = 0; i < conf.length; i++) {
            if (conf[i].id == fezID) {
                //console.log(pack);
                var h = 0;

                for (h = 0; h < conf[i].sensors.length; h++) {
                    config_sensor[h] = {
                        id: conf[i].sensors[h].id,
                        type: conf[i].sensors[h].name
                    };
                }
            }
        }

        Promise.all(allPromise).then(function (values) {
            //console.log(values);
            allMisure = [];
            label = [];
            index = 0;
            values.reverse();
            values.sort(function (a, b) { return a.LastModified < b.LastModified; });
            for (i = 0; i < values.length; i++) {
                if (values[i].Body.length == 0)
                    continue;
                var pack = JSON.parse(values[i].Body.toString("utf-8"));
                //console.log(pack);

                if (pack.device_id == fezID) {
                    //console.log(pack);
                    for (j = pack.measurements.length - 1; index < 30 && j >= 0; j--) {
                        /* if (pack.measurements[j].sensor == sensor) {
                             var misura = {
                                 x: pack.measurements[j].iso_timestamp,
                                 y: pack.measurements[j].value
                             }
                             //  console.log(misura);
                             allMisure[index] = misura;
                             //label[index] = misura.x.split('T')[1];
                             label[index] = misura.x;
                             index++;
                         }
                         else {*/
                        if (check_sensor(config_sensor, pack.measurements[j].sensor_id, sensor)) {
                            var misura = {
                                x: pack.measurements[j].iso_timestamp,
                                y: pack.measurements[j].value
                            }
                            // console.log(misura);
                            allMisure[index] = misura;
                            //label[index] = misura.x.split('T')[1];
                            label[index] = misura.x;
                            index++;
                        }

                        // }
                        //console.log(pack.measurement[i]);
                    }
                    // console.log(label);
                }
            }

            saveData(allMisure);
            label.sort();
            allMisure.sort(function (a, b) { return a.x.localeCompare(b.x); });

            //QUI HO TUTTI I DATI
            var datForm = {
                labels: label,
                datasets: [{
                    label: "Misure",
                    //pointRadius: 10,
                    data: allMisure,
                    // pointStyle: 'rectRounded',
                   // borderDash: [5, 5],
                    fill: true,
                    backgroundColor: "rgba(51,153,255,0.1)",
                    //backgroundColor:"rgb(54, 162, 235)",
                    borderColor:"rgb(54, 162, 235)"
                }]
            };
            var ctx = document.getElementById(chart);
            var myLineChart = new Chart(ctx, {
                type: 'line',
                data: datForm,
                options: {
                    responsive: true,
                    tooltips: {
                        mode: 'index',
                        intersect: false,
                    },
                    scales: {
                        xAxes: [{
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'TIMESTAMP'
                            }
                        }],
                        yAxes: [{
                            display: true,
                            scaleLabel: {
                                display: true,
                                labelString: 'VALUE'
                            }
                        }]
                    }

                }
            });

        });
    });

}


function check_sensor(configuration, id_sensor, type_sensor) {
    var i = 0;

    for (i = 0; i < configuration.length; i++) {
        if (configuration[i].id == id_sensor && configuration[i].type == type_sensor)
            return true;
    }

    return false;
}

function saveData(misure){
    AllData=misure;
}

function id2type(id,deviceConf)
{
    var name;
    deviceConf.sensors.forEach(function(element){

        if(element.id == id)
        name = element.name;
    });
    return name;
}
