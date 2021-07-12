using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Steamworks;
using Newtonsoft.Json;

namespace steam_matchmaking_servers {
    public class ServerInfo {
        public string addr { get; set; }
        public string name { get; set; }
        public int players { get; set; }
        public int max_players { get; set; }
        public string map { get; set; }
        public string[] tags { get; set; }
    }
    
    class Program {
        public static async Task Main(string[] args) {
            var result = new List<ServerInfo>();
            var appid = "108600";

            try {
                SteamClient.Init( 108600, true );
                
                var filter = new Steamworks.ServerList.Internet();
                
                filter.AddFilter("appid", appid);
                filter.AddFilter( "secure", "1" ); 
                
                void OnServersUpdated() {
                    // No responsive servers yet, bail
                    if ( filter.Responsive.Count == 0 ) {
                        return;
                    }

                    // Process each responsive server
                    foreach ( var s in filter.Responsive ) {
                        var tags = new string[] {};
                        if ( s.Tags != null && s.Tags.Length != 0 ) {
                            tags = s.Tags[0].Split(";");
                        }
                        
                        result.Add(new ServerInfo {
                            addr = s.Address + ":" + s.ConnectionPort,
                            name = s.Name,
                            players = s.Players,
                            max_players = s.MaxPlayers,
                            map = s.Map,
                            tags = tags
                        });
                    }

                    // Clear the responsive server list so we don't reprocess them on the next call.
                    filter.Responsive.Clear();
                }
                
                filter.OnChanges += OnServersUpdated;

                await filter.RunQueryAsync( 30 );
                filter.Cancel();
                
                SteamClient.Shutdown();
                
                result = result.OrderByDescending(o=>o.players).ToList();
                
                Console.WriteLine(JsonConvert.SerializeObject(result));
            } catch ( System.Exception e ) {
                Console.WriteLine(e.GetBaseException().Message);
                Console.WriteLine(e.StackTrace);
                
                return;
            }
        }
    }
}
