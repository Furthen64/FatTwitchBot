using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatTwitchBotA
{
  

  public class Twitchconfigfile
  {
    public string TwitchUsername { get; set; }
    public string OAUTH { get; set; }
    public int Timer1Seconds { get; set; }
    public int Timer2Seconds { get; set; }
    public int Timer3Seconds { get; set; }
  }



  public class TwitchConfig
  {
    public TwitchConfig(string jsonConfigFilename)
    {
      
      var config = new ConfigurationBuilder()
          .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
          .AddJsonFile(jsonConfigFilename).Build();


      var section = config.GetSection(nameof(Twitchconfigfile));
      var twConfig = section.Get<Twitchconfigfile>();
      
      this.password = twConfig.OAUTH;
      this.botUsername = twConfig.TwitchUsername; 
      this.Timer1Seconds = twConfig.Timer1Seconds;
      this.Timer2Seconds = twConfig.Timer2Seconds;
      this.Timer3Seconds = twConfig.Timer3Seconds;
      

    }


    public string password = "";
    public string botUsername = "";
    public string channelname = "";
    public int Timer1Seconds { get; set; }
    public int Timer2Seconds { get; set; }
    public int Timer3Seconds { get; set; }

  }



}
