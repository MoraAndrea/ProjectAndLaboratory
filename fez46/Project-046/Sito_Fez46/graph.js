function GraphCharts(sensor, chart, fezID) {

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
        Bucket: "polito.plcs" //idilio
    
        //MaxKeys: 10
    };
    //console.log(params1);
    s3.listObjects(params1, function (err, data) {
        var list = [];
         //console.log(data);
        // console.log(err);
        var allPromise = [];
        data.Contents.reverse();
        var listindex = 0;
       // for (i = 0; (i < data.Contents.length && i < 10); i++) {
        for (i = 0; (i < data.Contents.length); i++) {
            var n = data.Contents[i].Key
            //console.log(n);
            n = n.split('/');

           // if (n[0] == "configurations")
              //  continue;
            list[listindex] = data.Contents[i].Key;
            var p = {
                //Bucket: "my-iot-data2018",
                Bucket: "polito.plcs", //idilio
                Key: list[listindex]
            };

            allPromise[listindex++] = s3.getObject(p).promise();
        }
        console.log(val);
        Promise.all(allPromise).then(function (values) {
            //console.log(values);
            allMisure = [];
            label = [];
            index = 0;
            values.reverse();
            var config_sensor = [];
            for (i = 0; i < values.length; i++) {
                if (values[i].Body.length == 0)
                    continue;
                var pack = JSON.parse(values[i].Body.toString("utf-8"));
               if(pack.id == fezID){
                //console.log(pack);
                var h =0;
                
                for(h=0; h<pack.sensors.length; h++){
                    config_sensor[h] = {
                        id : pack.sensors[h].id ,
                        type : pack.sensors[h].type
                    } ;
                  
                  
                }
                
                
               }
                if (pack.device_id == fezID) {
                    //console.log(pack);
                    for (j = pack.measurements.length; index<30; j--) {
                           
                        if (pack.measurements[j].sensor == sensor) {
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
                        else
                        {
                            if(check_sensor(config_sensor,pack.measurements[j].sensor_id,sensor)){
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

                        }
                        //console.log(pack.measurement[i]);
                    }
                }
            }
           
            
            label.sort();
            allMisure.sort(function(a,b){return a.x.localeCompare(b.x); });
            console.log(label);
            console.log(allMisure);
            //QUI HO TUTTI I DATI
            var datForm = {
                labels: label,
                datasets: [{
                    label: "Misure",
                    //pointRadius: 10,
                    data: allMisure,
                   // pointStyle: 'rectRounded',
                    borderDash: [5, 5],
                    fill: true,
                    backgroundColor: "rgba(51,153,255,0.1)"
                }]


            };
            var ctx = document.getElementById(chart);
            var myLineChart = new Chart(ctx, {
                type: 'line',
                data: datForm,
                options: {
                    responsive: true

                }
            });

        });
    });

}


function  check_sensor(configuration,id_sensor,type_sensor){
    var i = 0;
    
    for(i=0; i<configuration.length; i++)
    {
        if(configuration[i].id == id_sensor && configuration[i].type==type_sensor  )
            return true;
    }

return false;
}
