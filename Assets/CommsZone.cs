using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading;
using System.Text;
using System.Linq;

public class CommsZone : MonoBehaviour
{
    Thread _t1;
    Thread _t2;
    GameObject commzone;
    GameObject satellite;
    float minDist = 70300f - 63710f;
    float maxDist = 20000f;
    bool isWithinComms;
    string msg;
    string msgQueue;
    IModel channelPub;
    // Start is called before the first frame update
    void Start()
    {
        isWithinComms = false;
        commzone = GameObject.FindGameObjectWithTag("CommZone");
        satellite = GameObject.FindGameObjectWithTag("Celestial");
        _t1 = new Thread(() => _func1("physicum"));
        _t1.Start();
        _t2 = new Thread(() => _func1("toravere"));
        _t2.Start();
        var factory = new ConnectionFactory() { HostName = "staging.estcube.eu", Port = 8008, UserName = "guest", Password = "guest" };
        var connection = factory.CreateConnection();
        channelPub = connection.CreateModel();
    }
    private void _func1(string queue)
    {
        var factory = new ConnectionFactory() { HostName = "staging.estcube.eu", Port = 8008, UserName = "guest", Password = "guest" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                if(isWithinComms) {
                    var body = ea.Body.ToArray();
                    msg = Encoding.UTF8.GetString(body);
                    msgQueue = queue;
                }
            };
            channel.BasicConsume(queue: queue,
                                 noAck: true,
                                 consumerTag: "CybexerUnity",
                                 consumer: consumer);
            while (true) ;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(msgQueue != null)
        {
            Debug.Log("Message is: " + msg);
            float chance = CalculateChance();
            float rand = Random.Range(0, 100f);
            Debug.Log("Chance was: " + chance + " and you rolled: " + rand);
            if (rand <= chance)
            {
                Debug.Log("Successful");
                    var properties = channelPub.CreateBasicProperties();
                    try
                    {
                        if (msgQueue == "physicum")
                        {
                            channelPub.BasicPublish(exchange: "ground_stations",
                                     routingKey: "uplink.toravere",
                                     basicProperties: properties,
                                     body: Encoding.UTF8.GetBytes(msg));
                        }
                        else
                        {
                            channelPub.BasicPublish(exchange: "ground_stations",
                                     routingKey: "uplink.physicum",
                                     basicProperties: properties,
                                     body: Encoding.UTF8.GetBytes(msg));
                        }
                    } catch (System.Exception e)
                    {
                        Debug.Log(e.Message);
                    };
            } else
            {
                Debug.Log("Unsuccessful");
            }
        }
        msgQueue = null;
    }

    float CalculateChance()
    {
        return (100 - ((100 * (Vector3.Distance(commzone.transform.position, satellite.transform.position) - minDist)) / (maxDist - minDist))) * 2;
    }

    private void OnTriggerEnter(Collider other)
    {
        isWithinComms = true;
        
    }
    private void OnTriggerExit(Collider other)
    {
        isWithinComms = false;
    }

}
