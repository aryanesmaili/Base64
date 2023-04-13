﻿using Base64.Utility.Models;
using Renci.SshNet.Messages;
using System.Security.Permissions;
using System.Text.Json;
using System.Web;

namespace Base64.Utility
{
    public static class Inbound
    {
        public enum PanelType
        {
            mhSanaei,
            FranzKafka
        }
        public static List<InboundModel> GetInbounds(this bool success, PanelType paneltype)
        {
            if (success && paneltype == PanelType.mhSanaei)
            {
                using (var context = new MhSanaeiConfigsContext())
                {
                    var inbounds = context.Inbounds.ToList();
                    if (inbounds.Any())
                    {
                        return inbounds;
                    }
                    throw new NullReferenceException();
                }
            }
            else if (success && paneltype == PanelType.FranzKafka)
            {
                using (var context = new KafkaConfigsContext())
                {
                    var inbounds = context.Inbounds.ToList();
                    if (inbounds.Any())
                    {
                        return inbounds;
                    }
                    throw new NullReferenceException();
                }
            }
            else
            {
                throw new NullReferenceException();
            }
        }
        #region Encode
        // Encode Section \\
        //public static string GetIPAddress(InboundModel inbound)
        //{
        //    string json = inbound.StreamSettings;
        //    var jObject = JsonDocument.Parse(json);

        //    // Access the "Host" value inside the "headers" object
        //    string host = jObject.RootElement
        //        .GetProperty("wsSettings")
        //        .GetProperty("headers")
        //        .GetProperty("Host").GetString()!;
        //    return host;
        //}

        //public static string GetSNI(InboundModel inbound)
        //{
        //    string json = inbound.StreamSettings;
        //    var jObject = JsonDocument.Parse(json);

        //    // Access the "serverName" value inside the "tlsSettings" object
        //    string sni = jObject.RootElement
        //        .GetProperty("tlsSettings")
        //        .GetProperty("serverName")
        //        .GetString()!;
        //    return sni;
        //}
        //public static string GetType(InboundModel inbound)
        //{
        //    var json = inbound.StreamSettings;
        //    var jsonObj = JsonDocument.Parse(json);

        //    var type = jsonObj.RootElement
        //        .GetProperty("network").GetString()!;

        //    return type;
        //}
        //public static string GetTrojanPassword(InboundModel inbound)
        //{
        //    var json = inbound.Settings;
        //    var jObject = JsonDocument.Parse(json);

        //    var TPassword = jObject.RootElement
        //        .GetProperty("clients")
        //        .GetProperty("password").GetString()!;

        //    return TPassword;
        //}
        public static string UrlEncode(string input)
        {
            return HttpUtility.UrlEncode(input);
        }
        //trojan://eWXHOiACe8@cf.sansorchi.net:2083?security=tls&sni=dns.sahel.live&alpn=http%2F1.1&fp=chrome&type=ws&host=dns
        //.sahel.live&path=%2Fwss20657%3Fed%3D2048%2F#%40NBA%20Trojan%20ws%20tls

        //public static List<T> GetClients<T>(InboundModel inbound) where T : class
        //{
        //    List<T> clients;
        //    string json = inbound.Settings;

        //    var options = new JsonSerializerOptions
        //    {
        //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        //        WriteIndented = true
        //    };

        //    try
        //    {
        //        var jsonDoc = JsonDocument.Parse(json);
        //        var clientsArray = jsonDoc.RootElement.GetProperty("clients");
        //        clients = JsonSerializer.Deserialize<List<T>>(clientsArray.GetRawText(), options)!;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //    return inbound.Settings.clients;
        //    return clients;
        //}


        public static List<string> VmessEncoder(InboundModel inbound)
        {
            var Clients = inbound.Settings.clients;

            List<string> VmessLinks = Clients.Select(Client =>
            "vmess://" +
            JsonSerializer.Serialize(new VmessBase64Model
            {
                v = "2",
                ps = inbound.Protocol + "-" + Client.email,
                add = inbound.StreamSettings.wsSettings.headers.Host,
                port = inbound.Port,
                id = Client.id.ToString(),
                aid = 0,
                scy = "auto",
                net = "ws",
                type = inbound.StreamSettings.network,
                path = inbound.StreamSettings.network == "ws" ? $"/wss{inbound.Port}" : $"grpc{inbound.Port}",
                tls = "tls",
                sni = inbound.StreamSettings.tlsSettings.serverName,
                alpn = ""
            })).ToList();

            return VmessLinks;
        }

        public static List<string> VlessGenerator(InboundModel inbound)
        {
            var Clients = inbound.Settings.clients;

            List<string> VlessLinks = Clients.Select(Client => new VlessModel
            {
                UUID = Client.id,
                Address = inbound.StreamSettings.wsSettings.headers.Host,
                Port = inbound.Port,
                type = inbound.StreamSettings.network,
                security = "tls",
                path = inbound.StreamSettings.network == "ws" ? $"/wss{inbound.Port}" : $"grpc{inbound.Port}",
                host = inbound.StreamSettings.tlsSettings.serverName,
                SNI = inbound.StreamSettings.tlsSettings.serverName,
                Remark = "#" + inbound.Protocol + "-" + Client.email

            }.ToVlessLink()).ToList();

            return VlessLinks;
        }
        public static List<string> TrojanGenerator(InboundModel inbound)
        {
            var Clients = inbound.Settings.clients;

            List<string> VlessLinks = Clients.Select(Client => new TrojanModel
            {
                UUID = Client.password,
                Address = inbound.StreamSettings.wsSettings.headers.Host,
                Port = inbound.Port,
                type = inbound.StreamSettings.network,
                security = "tls",
                path = $"/wss{inbound.Port}",
                host = inbound.StreamSettings.tlsSettings.serverName,
                SNI = inbound.StreamSettings.tlsSettings.serverName,
                Remark = "#" + inbound.Protocol + "-" + Client.email,
                serviceName = $"grpc{inbound.Port}",
                alpn = inbound.StreamSettings.network == "ws" ? "http/1.1" : "h2",
                fp = "chrome",
                mode = "gun"

            }.ToTrojanLink()).ToList();

            return VlessLinks;
        }

        public static string ToVlessLink(this VlessModel VlessObject)
        {
            return $"vless://{VlessObject.UUID}@{VlessObject.Address}:{VlessObject.Port}?sni={VlessObject.SNI}&security={VlessObject.security}&type={VlessObject.type}" +
                $"&path={VlessObject.path}&host={VlessObject.host}{VlessObject.Remark}";
        }

        public static string ToTrojanLink(this TrojanModel trojan)
        {
            return $"trojan://{trojan.UUID}@{trojan.Address}:{trojan.Port}?security={trojan.security}&sni={trojan.SNI}&" +
                $"alpn={((trojan.type == "ws") ? UrlEncode(trojan.alpn) : trojan.alpn)}&fp={trojan.fp}&" +
         $"type={trojan.type}&{((trojan.type == "ws") ? $"host={trojan.host}&path={UrlEncode(trojan.path)}{UrlEncode(trojan.Remark)}" : $"serviceName={trojan.serviceName}&mode={trojan.mode}{UrlEncode(trojan.Remark)}")}";
        }

        // End Section \\
        #endregion
        // IP Section \\
        public static async Task<List<string>> ToIPList(this string path)
        {
            List<string> IPs = new();
            string content;

            using (StreamReader reader = new(path))
            {
                content = await reader.ReadToEndAsync();
            }
            var IPlist = content.Contains(',') ? content.Split(",") : content.Split(' ');
            IPs.AddRange(IPlist);
            return IPs;
        }
        public static List<List<InboundModel>> PermutateIPS(List<InboundModel> inbounds, List<string> IPs)
        {
            /*
            This function takes in a list of inbound objects and a list of IP addresses.
            It then generates all the possible combinations of the inbound objects with the given IP addresses.
            The function uses LINQ to generate the combinations by iterating over the inbound objects and for each object,
            it creates new inbound objects by replacing the IP address in the StreamSettings field with each IP address in the given list.
            The new inbound objects are then added to a list of lists, where each inner list represents a combination of the inbound objects with a particular IP address.
            Finally, the function returns the list of all combinations.
            */
            var resInbounds = new List<List<InboundModel>>();

            foreach (var inbound in inbounds)
            {
                var newInbounds = IPs.Select(ip =>
                {
                    // TODO: Optimize this
                    var newInbound = new InboundModel
                    {
                        Id = inbound.Id,
                        UserId = inbound.UserId,
                        Up = inbound.Up,
                        Down = inbound.Down,
                        Total = inbound.Total,
                        Remark = inbound.Remark,
                        Enable = inbound.Enable,
                        ExpiryTime = inbound.ExpiryTime,
                        Listen = inbound.Listen,
                        Port = inbound.Port,
                        Protocol = inbound.Protocol,
                        Settings = inbound.Settings,
                        StreamSettings = inbound.StreamSettings,
                        Tag = inbound.Tag,
                        Sniffing = inbound.Sniffing
                    };
                    newInbound.StreamSettings.wsSettings.headers.Host = ip;
                    return newInbound;
                }).ToList();
                resInbounds.Add(newInbounds);
            }
            return resInbounds;
        }
    }
}