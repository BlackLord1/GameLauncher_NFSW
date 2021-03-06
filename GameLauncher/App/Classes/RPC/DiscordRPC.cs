﻿using GameLauncherReborn;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace GameLauncher.App.Classes.RPC {
    class DiscordRPC {
        public static long RPCstartTimestamp = 0000000000;
        public static DiscordRpc.RichPresence _presence = new DiscordRpc.RichPresence();
        public static DiscordRpc.EventHandlers handlers = new DiscordRpc.EventHandlers();

        //Some checks
        private static string serverName = ServerProxy.Instance.GetServerName();
        private static bool canUpdateProfileField = false;
        private static bool eventTerminatedManually = false;
        private static int EventID;
        private static string carslotsXML = String.Empty;

        public DiscordRPC() {
            DiscordRpc.Initialize("427355155537723393", ref handlers, true, "");
            Console.WriteLine("INITIALIZED!");
        }

        //Some data related, can be touched.
        public static string PersonaId = String.Empty;
        public static string PersonaName = String.Empty;
        public static string PersonaLevel = String.Empty;
        public static string PersonaAvatarId = String.Empty;
        public static string PersonaCarId = String.Empty;
        public static string PersonaCarName = String.Empty;
        public static List<string> PersonaIds = new List<string>();

        public static void handleGameState(string uri, string serverreply = "", string POST = "", string GET = "") {
            var SBRW_XML = new XmlDocument();
            string[] splitted_uri = uri.Split('/');

            if (uri == "/User/SecureLoginPersona") {
                canUpdateProfileField = true;
            }
            if (uri == "/User/SecureLogoutPersona") {
                PersonaId = String.Empty;
                PersonaName = String.Empty;
                PersonaLevel = String.Empty;
                PersonaAvatarId = String.Empty;
                PersonaCarId = String.Empty;
                PersonaCarName = String.Empty;
            }

            //FIRST PERSONA EVER LOCALIZED IN CODE
            if (uri == "/User/GetPermanentSession") {
                try {
                    SBRW_XML.LoadXml(serverreply);

                    PersonaName = SBRW_XML.SelectSingleNode("UserInfo/personas/ProfileData/Name").InnerText.Replace("¤", "[S]");
                    PersonaLevel = SBRW_XML.SelectSingleNode("UserInfo/personas/ProfileData/Level").InnerText;
                    PersonaAvatarId = (SBRW_XML.SelectSingleNode("UserInfo/personas/ProfileData/IconIndex").InnerText == "26") ? "nfsw" : "avatar_" + SBRW_XML.SelectSingleNode("UserInfo/personas/ProfileData/IconIndex").InnerText;
                    PersonaId = SBRW_XML.SelectSingleNode("UserInfo/personas/ProfileData/PersonaId").InnerText;

                    //Let's get rest of PERSONAIDs
                    XmlNode UserInfo = SBRW_XML.SelectSingleNode("UserInfo");
                    XmlNodeList personas = UserInfo.SelectNodes("personas/ProfileData");
                    foreach (XmlNode node in personas) {
                        PersonaIds.Add(node.SelectSingleNode("PersonaId").InnerText);
                    }
                } catch (Exception) {

                }
            }

            //CREATE/DELETE PERSONA Handler
            if (uri == "/DriverPersona/CreatePersona") {
                SBRW_XML.LoadXml(serverreply);
                PersonaIds.Add(SBRW_XML.SelectSingleNode("ProfileData/PersonaId").InnerText);
            }

            //DRIVING CARNAME
            if (uri == "/DriverPersona/GetPersonaInfo" && canUpdateProfileField == true) {
                SBRW_XML.LoadXml(serverreply);
                PersonaName = SBRW_XML.SelectSingleNode("ProfileData/Name").InnerText.Replace("¤", "[S]");
                PersonaLevel = SBRW_XML.SelectSingleNode("ProfileData/Level").InnerText;
                PersonaAvatarId = (SBRW_XML.SelectSingleNode("ProfileData/IconIndex").InnerText == "26") ? "nfsw" : "avatar_" + SBRW_XML.SelectSingleNode("ProfileData/IconIndex").InnerText;
                PersonaId = SBRW_XML.SelectSingleNode("ProfileData/PersonaId").InnerText;
            }
            if (uri == "/matchmaking/leavelobby") {
                _presence.details = "Driving " + PersonaCarName;
                _presence.state = serverName;
                _presence.largeImageText = PersonaName + " - Level: " + PersonaLevel;
                _presence.largeImageKey = PersonaAvatarId;
                _presence.smallImageText = "In-Freeroam";
                _presence.smallImageKey = "gamemode_freeroam";
                _presence.startTimestamp = RPCstartTimestamp;
                _presence.instance = true;
                DiscordRpc.UpdatePresence(_presence);

                eventTerminatedManually = true;
            }

            //IN LOBBY
            if (uri == "/matchmaking/acceptinvite") {
                SBRW_XML.LoadXml(serverreply);
                EventID = Convert.ToInt32(SBRW_XML.SelectSingleNode("LobbyInfo/EventId").InnerText);

                _presence.details = "In Lobby: " + EventList.getEventName(EventID);
                _presence.state = serverName;
                _presence.largeImageText = PersonaName + " - Level: " + PersonaLevel;
                _presence.largeImageKey = PersonaAvatarId;
                _presence.smallImageText = EventList.getEventName(Convert.ToInt32(EventID));
                _presence.smallImageKey = EventList.getEventType(Convert.ToInt32(EventID));
                _presence.startTimestamp = RPCstartTimestamp;
                _presence.instance = true;
                DiscordRpc.UpdatePresence(_presence);

                eventTerminatedManually = false;
            }

            //IN SAFEHOUSE/FREEROAM
            if (uri == "/DriverPersona/UpdatePersonaPresence") {
                string UpdatePersonaPresenceParam = GET.Split(';').Last().Split('=').Last();
                if(UpdatePersonaPresenceParam == "1") {
                    _presence.details = "Driving " + PersonaCarName;
                    _presence.smallImageText = "In-Freeroam";
                } else {
                    _presence.details = "In Safehouse";
                    _presence.smallImageText = "In-Safehouse";
                }

                _presence.state = serverName;
                _presence.largeImageText = PersonaName + " - Level: " + PersonaLevel;
                _presence.largeImageKey = PersonaAvatarId;
                _presence.smallImageKey = "gamemode_freeroam";
                _presence.startTimestamp = RPCstartTimestamp;
                _presence.instance = true;
                DiscordRpc.UpdatePresence(_presence);
            }

            //IN EVENT
            if (Regex.Match(uri, "/matchmaking/launchevent").Success) {
                EventID = Convert.ToInt32(splitted_uri[3]);

                _presence.details = "In Event: " + EventList.getEventName(EventID);
                _presence.state = serverName;
                _presence.largeImageText = PersonaName + " - Level: " + PersonaLevel;
                _presence.largeImageKey = PersonaAvatarId;
                _presence.smallImageText = EventList.getEventName(EventID);
                _presence.smallImageKey = EventList.getEventType(EventID);
                _presence.startTimestamp = RPCstartTimestamp;
                _presence.instance = true;
                DiscordRpc.UpdatePresence(_presence);

                eventTerminatedManually = false;
            }
            if (uri == "/event/arbitration") {
                _presence.details = "In Event: " + EventList.getEventName(EventID);
                _presence.state = serverName;
                _presence.largeImageText = PersonaName + " - Level: " + PersonaLevel;
                _presence.largeImageKey = PersonaAvatarId;
                _presence.smallImageText = EventList.getEventName(EventID);
                _presence.smallImageKey = EventList.getEventType(EventID);
                _presence.startTimestamp = RPCstartTimestamp;
                _presence.instance = true;
                DiscordRpc.UpdatePresence(_presence);

                eventTerminatedManually = false;
            }
            if (uri == "/event/launched" && eventTerminatedManually == false) {
                _presence.details = "In Event: " + EventList.getEventName(EventID);
                _presence.state = serverName;
                _presence.largeImageText = PersonaName + " - Level: " + PersonaLevel;
                _presence.largeImageKey = PersonaAvatarId;
                _presence.smallImageText = EventList.getEventName(EventID);
                _presence.smallImageKey = EventList.getEventType(EventID);
                _presence.startTimestamp = Self.getTimestamp(true);
                _presence.instance = true;
                DiscordRpc.UpdatePresence(_presence);
            }

            //CARS RELATED
            foreach (var single_personaId in PersonaIds) {
                if (Regex.Match(uri, "/personas/" + single_personaId + "/carslots", RegexOptions.IgnoreCase).Success) {
                    carslotsXML = serverreply;

                    SBRW_XML.LoadXml(carslotsXML);

                    int DefaultID = Convert.ToInt32(SBRW_XML.SelectSingleNode("CarSlotInfoTrans/DefaultOwnedCarIndex").InnerText);
                    int current = 0;

                    XmlNode CarsOwnedByPersona = SBRW_XML.SelectSingleNode("CarSlotInfoTrans/CarsOwnedByPersona");
                    XmlNodeList OwnedCarTrans = CarsOwnedByPersona.SelectNodes("OwnedCarTrans");

                    foreach (XmlNode node in OwnedCarTrans) {
                        if(DefaultID == current) {
                            PersonaCarName = CarList.getCarName(node.SelectSingleNode("CustomCar/Name").InnerText);
                        }
                        current++;
                    }
                }
                if (Regex.Match(uri, "/personas/" + single_personaId + "/defaultcar", RegexOptions.IgnoreCase).Success) {
                    if(splitted_uri.Last() != "defaultcar") {
                        string receivedId = splitted_uri.Last();

                        SBRW_XML.LoadXml(carslotsXML);
                        XmlNode CarsOwnedByPersona = SBRW_XML.SelectSingleNode("CarSlotInfoTrans/CarsOwnedByPersona");
                        XmlNodeList OwnedCarTrans = CarsOwnedByPersona.SelectNodes("OwnedCarTrans");

                        foreach (XmlNode node in OwnedCarTrans) {
                            if (receivedId == node.SelectSingleNode("Id").InnerText) {
                                PersonaCarName = CarList.getCarName(node.SelectSingleNode("CustomCar/Name").InnerText);
                            }
                        }
                    }
                }
            }
        }
    }
}