//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WcfMineServer
{
    using System;
    using System.Collections.Generic;
    
    public partial class Game
    {
        public int Serial { get; set; }
        public string Winner { get; set; }
        public string Loser { get; set; }
        public bool Tie { get; set; }
        public System.DateTime GameEnded { get; set; }
    }
}
