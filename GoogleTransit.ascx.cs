using System;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Diagnostics;
using umbraco.uicontrols;
using umbraco;
using System.Configuration;
using System.Web.Configuration;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using umbraco.DataLayer;
using umbraco.interfaces;
using umbraco.cms.helpers;
using umbraco.cms.businesslogic.datatype.controls;
using umbraco.DataLayer.Utility;
using umbraco.controls;
using umbraco.controls.GenericProperties;
using umbraco.controls.datagrid;
using umbraco.editorControls;
using umbraco.layoutControls;
using umbraco.presentation.nodeFactory;
using Ionic.Zip;
using System.Web.UI.HtmlControls;
using System.Xml;

namespace GoogleTransit
{
    public partial class GoogleTransit : System.Web.UI.UserControl, umbraco.editorControls.userControlGrapper.IUsercontrolDataEditor
    {
        public string umbracoValue;

        protected void Page_Load(object sender, EventArgs e)
        {
            isFeedPublished(); // Call function to see if the transit feed has been published or not
        }

        private void isFeedPublished()
        {
            string loc = ConfigurationManager.AppSettings["transitLocation"].ToString();
            string arch = ConfigurationManager.AppSettings["transitArchive"].ToString();
            string appRoot = HttpRuntime.AppDomainAppPath;
            string transitDirectory = loc.Replace("~/", appRoot);

            System.IO.FileInfo feedInf = new System.IO.FileInfo(transitDirectory + "\\" + arch);

            if (!IsPostBack)
            {
                string id = Request["id"];

                if (!feedInf.Exists)
                {
                    status.Text = "No feed found.";
                }
                else
                {
                    status.Text = "Feed published - Last updated: " + feedInf.LastWriteTime.ToString("dddd, MMMM dd, yyyy hh:mm tt");
                }
            }
        }

        public object value
        {
            get
            {
                return umbracoValue;
            }
            set
            {
                umbracoValue = value.ToString();
            }
        }

        protected override void OnInit(EventArgs e) { }

        protected void Button1_Click(object sender, System.EventArgs e)
        {
            string loc = ConfigurationManager.AppSettings["transitLocation"].ToString();
            string arch = ConfigurationManager.AppSettings["transitArchive"].ToString();
            string appRoot = HttpRuntime.AppDomainAppPath;
            string transitDirectory = loc.Replace("~/", appRoot);
            System.IO.StreamWriter agencyRecord, feedInfoRecord, routesRecord, stopsRecord, calendarsRecord, calendarDatesRecord, tripsRecord, stopTimesRecord, fareAttributesRecord, fareRulesRecord, testingRecord;

            // Set the message text back to nothing
            message.Text = " ";

            // Create root variables
            int agencyRoot;
            int routesRoot;
            int servicesRoot;
            int exceptionsRoot;
            int stopsRoot;
            int tripsRoot;
            int faresRoot;

            // Create an Error Counter
            int errors = 0;

            // Check to see if each of the txt files exist. If they don't then create them
            if (!System.IO.File.Exists(transitDirectory + "\\agency.txt"))
            {
                System.IO.File.Create(transitDirectory + "\\agency.txt");
            }
            if (!System.IO.File.Exists(transitDirectory + "\\feed_info.txt"))
            {
                System.IO.File.Create(transitDirectory + "\\feed_info.txt");
            }
            if (!System.IO.File.Exists(transitDirectory + "\\routes.txt"))
            {
                System.IO.File.Create(transitDirectory + "\\routes.txt");
            }
            if (!System.IO.File.Exists(transitDirectory + "\\stops.txt"))
            {
                System.IO.File.Create(transitDirectory + "\\stops.txt");
            }
            if (!System.IO.File.Exists(transitDirectory + "\\calendar.txt"))
            {
                System.IO.File.Create(transitDirectory + "\\calendar.txt");
            }
            if (!System.IO.File.Exists(transitDirectory + "\\calendar_dates.txt"))
            {
                System.IO.File.Create(transitDirectory + "\\calendar_dates.txt");
            }
            if (!System.IO.File.Exists(transitDirectory + "\\trips.txt"))
            {
                System.IO.File.Create(transitDirectory + "\\trips.txt");
            }
            if (!System.IO.File.Exists(transitDirectory + "\\stop_times.txt"))
            {
                System.IO.File.Create(transitDirectory + "\\stop_times.txt");
            }
            if (!System.IO.File.Exists(transitDirectory + "\\fare_attributes.txt"))
            {
                System.IO.File.Create(transitDirectory + "\\fare_attributes.txt");
            }
            if (!System.IO.File.Exists(transitDirectory + "\\fare_rules.txt"))
            {
                System.IO.File.Create(transitDirectory + "\\fare_rules.txt");
            }
            if (!System.IO.File.Exists(transitDirectory + "\\testing.txt"))
            {
                System.IO.File.Create(transitDirectory + "\\testing.txt");
            }

            try
            {
                // Create StreamWriters for each of the txt Records
                agencyRecord = new StreamWriter(transitDirectory + "\\agency.txt");
                feedInfoRecord = new StreamWriter(transitDirectory + "\\feed_info.txt");
                routesRecord = new StreamWriter(transitDirectory + "\\routes.txt");
                stopsRecord = new StreamWriter(transitDirectory + "\\stops.txt");
                calendarsRecord = new StreamWriter(transitDirectory + "\\calendar.txt");
                calendarDatesRecord = new StreamWriter(transitDirectory + "\\calendar_dates.txt");
                tripsRecord = new StreamWriter(transitDirectory + "\\trips.txt");
                stopTimesRecord = new StreamWriter(transitDirectory + "\\stop_times.txt");
                fareAttributesRecord = new StreamWriter(transitDirectory + "\\fare_attributes.txt");
                fareRulesRecord = new StreamWriter(transitDirectory + "\\fare_rules.txt");
                testingRecord = new StreamWriter(transitDirectory + "\\testing.txt");

                // Write the first line of each txt file
                agencyRecord.WriteLine("agency_id,agency_name,agency_url,agency_timezone,agency_lang,agency_phone,agency_fare_url");
                feedInfoRecord.WriteLine("feed_publisher_name,feed_publisher_url,feed_lang,feed_version");
                routesRecord.WriteLine("route_id,agency_id,route_short_name,route_long_name,route_desc,route_type,route_url,route_color,route_text_color");
                stopsRecord.WriteLine("stop_id,stop_code,stop_name,stop_desc,stop_lat,stop_lon,zone_id,stop_url,location_type,parent_station");
                calendarsRecord.WriteLine("service_id,monday,tuesday,wednesday,thursday,friday,saturday,sunday,start_date,end_date");
                calendarDatesRecord.WriteLine("service_id,date,exception_type");
                tripsRecord.WriteLine("route_id,service_id,trip_id,trip_headsign,direction_id,block_id,shape_id");
                stopTimesRecord.WriteLine("trip_id,arrival_time,departure_time,stop_id,stop_sequence,stop_headsign,pickup_type,drop_off_type,shape_dist_traveled");
                fareAttributesRecord.WriteLine("fare_id,price,currency_type,payment_method,transfers,transfer_duration");
                fareRulesRecord.WriteLine("fare_id,route_id,origin_id,destination_id,contains_id");

                // Connect to the SQL Database
                SqlConnection SQLConn = new SqlConnection(GlobalSettings.DbDSN);
                SQLConn.Open();
                SqlDataAdapter dataAdapter;
                DataTableReader dataReader;

                // Connect to the SQL database for the Routes node
                string agencySQL = string.Format(@"select id from umbracoNode where text = 'Routes'");
                dataAdapter = new SqlDataAdapter(agencySQL, SQLConn);
                DataSet agencyDataSet = new DataSet();
                dataAdapter.Fill(agencyDataSet);

                // Connect to the SQL database for the Transit Routes node
                string routesSQL = string.Format(@"select id from umbracoNode where text = 'Transit Routes'");
                dataAdapter = new SqlDataAdapter(routesSQL, SQLConn);
                DataSet routesDataSet = new DataSet();
                dataAdapter.Fill(routesDataSet);

                // Connect to the SQL database for the Transit Service Calendars node
                string servicesSQL = string.Format(@"select id from umbracoNode where text = 'Transit Service Calendars'");
                dataAdapter = new SqlDataAdapter(servicesSQL, SQLConn);
                DataSet servicesDataSet = new DataSet();
                dataAdapter.Fill(servicesDataSet);

                // Connect to the SQL database for the Transit Service Calendar Exceptions node
                string exceptionsSQL = string.Format(@"select id from umbracoNode where text = 'Transit Service Calendar Exceptions'");
                dataAdapter = new SqlDataAdapter(exceptionsSQL, SQLConn);
                DataSet exceptionsDataSet = new DataSet();
                dataAdapter.Fill(exceptionsDataSet);

                // Connect to the SQL database for the Transit Stops node
                string stopsSQL = string.Format(@"select id from umbracoNode where text = 'Transit Stops'");
                dataAdapter = new SqlDataAdapter(stopsSQL, SQLConn);
                DataSet stopsDataSet = new DataSet();
                dataAdapter.Fill(stopsDataSet);

                // Connect to the SQL database for the Transit Trips node
                string tripsSQL = string.Format(@"select id from umbracoNode where text = 'Transit Trips and Stop Times'");
                dataAdapter = new SqlDataAdapter(tripsSQL, SQLConn);
                DataSet tripsDataSet = new DataSet();
                dataAdapter.Fill(tripsDataSet);

                // Connect to the SQL database for the Transit Fares node
                string faresSQL = string.Format(@"select id from umbracoNode where text = 'Transit Fares'");
                dataAdapter = new SqlDataAdapter(faresSQL, SQLConn);
                DataSet faresDataSet = new DataSet();
                dataAdapter.Fill(faresDataSet);

                // Close and dispose SQL connection
                SQLConn.Close();
                SQLConn.Dispose();

                // Set the data reader for the Routes node and set the ID for the Routes node
                dataReader = agencyDataSet.CreateDataReader();
                if (dataReader.Read()) { agencyRoot = (int)dataReader["id"]; } else { status.Text = "No Routes Folder Found!"; return; }

                // Set the data reader for the Transit Routes node and set the ID for the Transit Routes node
                dataReader = routesDataSet.CreateDataReader();
                if (dataReader.Read()) { routesRoot = (int)dataReader["id"]; } else { status.Text = "No Transit Routes Folder Found!"; return; }

                // Set the data reader for the Transit Service Calendars node and set the ID for the Transit Service Calendars node
                dataReader = servicesDataSet.CreateDataReader();
                if (dataReader.Read()) { servicesRoot = (int)dataReader["id"]; } else { status.Text = "No Transit Service Calendars Folder Found!"; return; }

                // Set the data reader for the Transit Service Calendars Exceptions node and set the ID for the Transit Service Calendars Exceptions node
                dataReader = exceptionsDataSet.CreateDataReader();
                if (dataReader.Read()) { exceptionsRoot = (int)dataReader["id"]; } else { status.Text = "No Transit Service Calendar Exceptions Folder Found!"; return; }

                // Set the data reader for the Transit Stops node and set the ID for the Transit Stops node
                dataReader = stopsDataSet.CreateDataReader();
                if (dataReader.Read()) { stopsRoot = (int)dataReader["id"]; } else { status.Text = "No Transit Stops Folder Found!"; return; }

                // Set the data reader for the Transit Trips node and set the ID for the Transit Trips and Stop Times node
                dataReader = tripsDataSet.CreateDataReader();
                if (dataReader.Read()) { tripsRoot = (int)dataReader["id"]; } else { status.Text = "No Transit Trips and Stop Times Folder Found!"; return; }

                // Set the data reader for the Transit Fares node and set the ID for the Transit Fares node
                dataReader = faresDataSet.CreateDataReader();
                if (dataReader.Read()) { faresRoot = (int)dataReader["id"]; } else { status.Text = "No Transit Fares Folder Found!"; return; }

                // Dispose dataReader
                dataReader.Dispose();

                // Get the agency.txt information from the Routes node and write it to agencyRecord
                Document agency = new Document(agencyRoot);
                // Create URL out of agency fare page id
                string fareURL = new Node(Convert.ToInt32(agency.getProperty("agencyFarePage").Value.ToString())).NiceUrl;
                agencyRecord.WriteLine(agency.getProperty("agencyID").Value.ToString() + "," + agency.getProperty("agencyName").Value.ToString().Replace(",", " ") + "," + agency.getProperty("agencyURL").Value.ToString() + "," + agency.getProperty("agencyTimezone").Value.ToString() + ",EN," + agency.getProperty("agencyPhoneNumber").Value.ToString() + "," + agency.getProperty("agencyURL").Value.ToString() + fareURL);
                // Testing 
                testingRecord.WriteLine("agency.txt");
                testingRecord.WriteLine("agency_id = " + agency.getProperty("agencyID").Value.ToString() + "  |  agency_name = " + agency.getProperty("agencyName").Value.ToString().Replace(",", " ") + "  |  agency_url = " + agency.getProperty("agencyURL").Value.ToString() + "  |  agency_timezone = " + agency.getProperty("agencyTimezone").Value.ToString() + "  |  agency_lang = EN  |  agency_phone = " + agency.getProperty("agencyPhoneNumber").Value.ToString() + "  |  agency_fare_url = " + agency.getProperty("agencyURL").Value.ToString() + fareURL);
                testingRecord.WriteLine("");


                // Write the only line for feed_info.txt
                feedInfoRecord.WriteLine("HillSouth,http://hillsouth.com,EN,");
                // Testing
                testingRecord.WriteLine("feed_info.txt");
                testingRecord.WriteLine("feed_publisher_name = HillSouth  |  feed_publisher_url = http://hillsouth.com  |  feed_lang = EN  |  feed_version =  ");
                testingRecord.WriteLine("");


                // Get the routes.txt information from the Transit Routes children nodes and write it to routesRecord
                Document[] routes = new Document(routesRoot).Children;
                // Testing
                testingRecord.WriteLine("routes.txt");
                for (int a = 0; a < routes.Length; a++)
                {
                    // Send the route to XML
                    XmlNode routesNode = routes[a].ToXml(new XmlDocument(), false);
                    // Check to see if the routesNode equals null, if so, the route is not published
                    string routeNodeName;
                    if (routesNode == null)
                    {
                        continue;
                    }
                    else
                    {
                        // Get the route node name from the xml node attributes
                        routeNodeName = routesNode.Attributes.GetNamedItem("nodeName").Value.ToString();
                    }
                    // Check to see if the Route needs to be excludes due to the googleTransitOverride
                    string googleTransitOverride = routes[a].getProperty("googleTransitOverride").Value.ToString();
                    if (googleTransitOverride == "1")
                    {
                        message.Text = message.Text + "&#8226; <span style='color: red;'>Route <b>" + routeNodeName + "</b> has not been added to the Google Transit Feed.</span><br />";
                        continue;
                    }
                    // Write the needed line
                    routesRecord.WriteLine(routeNodeName + ",," + routes[a].getProperty("routeShortName").Value.ToString().Replace(",", " ") + "," + routes[a].getProperty("routeFullName").Value.ToString().Replace(",", " ") + "," + routes[a].getProperty("routeDescription").Value.ToString().Replace(",", " ") + ",3,,,");
                    // Testing
                    testingRecord.WriteLine("route_id = " + routeNodeName + "  |  agency_id =  |  route_short_name = " + routes[a].getProperty("routeShortName").Value.ToString().Replace(",", " ") + "  |  route_long_name = " + routes[a].getProperty("routeFullName").Value.ToString().Replace(",", " ") + "  |  route_description = " + routes[a].getProperty("routeDescription").Value.ToString().Replace(",", " ") + "  |  route_type = 3  |  route_url =  |  route_color =  |  route_text_color = ");
                }
                // Testing
                testingRecord.WriteLine("");


                // Get the stops.txt information from the Transit Stops children nodes and write it to stopsRecord
                Document[] stops = new Document(stopsRoot).Children;
                // Testing
                testingRecord.WriteLine("stops.txt");
                for (int b = 0; b < stops.Length; b++)
                {
                    // Send the stop to XML
                    XmlNode stopsNode = stops[b].ToXml(new XmlDocument(), false);
                    // Check to see if the stopsNode equals null, if so, the stop is not published
                    string stopNodeName;
                    if (stopsNode == null)
                    {
                        continue;
                    }
                    else
                    {
                        // Get the stop node name from the xml node attributes
                        stopNodeName = stopsNode.Attributes.GetNamedItem("nodeName").Value.ToString();
                    }
                    // Create stopCoordinates variable
                    string[] stopCoordinates;
                    // Check to see whether or not stopMap has been populated
                    if (String.IsNullOrEmpty(stops[b].getProperty("stopMap").Value.ToString()))
                    {
                        message.Text = message.Text + "&#8226; <span style='color: red;'>Stop <b>" + stopNodeName + "</b> has not been mapped correctly.</span><br />";
                        errors++;
                        continue;
                    }
                    else
                    {
                        stopCoordinates = stops[b].getProperty("stopMap").Value.ToString().Split(',');
                    }
                    // Check to see whether or not stopZone has been populated
                    if (stops[b].getProperty("stopZone").Value.ToString() == "-1")
                    {
                        message.Text = message.Text + "&#8226; <span style='color: red;'>Stop <b>" + stopNodeName + "</b> does not have a stop zone applied to it.</span><br />";
                        errors++;
                        continue;
                    }
                    // Write the information
                    stopsRecord.WriteLine(stopNodeName + ",," + stops[b].getProperty("stopName").Value.ToString().Replace(",", " ") + "," + stops[b].getProperty("stopDescription").Value.ToString().Replace(",", " ") + "," + stopCoordinates[0] + "," + stopCoordinates[1] + "," + stops[b].getProperty("stopZone").Value.ToString() + ",,,");
                    // Testing
                    testingRecord.WriteLine("stop_id = " + stopNodeName + "  |  stop_code =  |  stop_name = " + stops[b].getProperty("stopName").Value.ToString().Replace(",", " ") + "  |  stop_desc = " + stops[b].getProperty("stopDescription").Value.ToString().Replace(",", " ") + "  |  stop_lat = " + stopCoordinates[0] + "  |  stop_lon = " + stopCoordinates[1] + "  |  zone_id = " + stops[b].getProperty("stopZone").Value.ToString() + "  |  stop_url =  |  location_type =  |  parent_station = ");
                }
                // Testing
                testingRecord.WriteLine("");


                // Get the calendar.txt information from the Transit Service Calendars children nodes and write it to calendarsRecord
                Document[] services = new Document(servicesRoot).Children;
                // Testing
                testingRecord.WriteLine("calendar.txt");
                for (int c = 0; c < services.Length; c++)
                {
                    // Send the calendar to XML
                    XmlNode servicesNode = services[c].ToXml(new XmlDocument(), false);
                    // Check to see if the servicesNode equals null, if so, the exception is not published
                    string serviceNodeName;
                    if (servicesNode == null)
                    {
                        continue;
                    }
                    else
                    {
                        // Get the calendar node name from the xml node attributes
                        serviceNodeName = servicesNode.Attributes.GetNamedItem("nodeName").Value.ToString();
                    }
                    // Check to see if the service calendar has at least one day selected
                    if (services[c].getProperty("serviceMonday").Value.ToString() == "0" && services[c].getProperty("serviceTuesday").Value.ToString() == "0" && services[c].getProperty("serviceWednesday").Value.ToString() == "0" && services[c].getProperty("serviceThursday").Value.ToString() == "0" && services[c].getProperty("serviceFriday").Value.ToString() == "0" && services[c].getProperty("serviceSaturday").Value.ToString() == "0" && services[c].getProperty("serviceSunday").Value.ToString() == "0")
                    {
                        message.Text = message.Text + "&#8226; <span style='color: red;'>Service Calendar <b>" + serviceNodeName + "</b> does not have any days chosen. A Service Calendar must have at least one day chosen.</span><br />";
                        errors++;
                        continue;
                    }
                    // Write the information
                    calendarsRecord.WriteLine(serviceNodeName + "," + services[c].getProperty("serviceMonday").Value.ToString() + "," + services[c].getProperty("serviceTuesday").Value.ToString() + "," + services[c].getProperty("serviceWednesday").Value.ToString() + "," + services[c].getProperty("serviceThursday").Value.ToString() + "," + services[c].getProperty("serviceFriday").Value.ToString() + "," + services[c].getProperty("serviceSaturday").Value.ToString() + "," + services[c].getProperty("serviceSunday").Value.ToString() + "," + services[c].getProperty("serviceStartDate").Value.ToString().Substring(6, 4) + services[c].getProperty("serviceStartDate").Value.ToString().Substring(3, 2) + services[c].getProperty("serviceStartDate").Value.ToString().Substring(0, 2) + "," + services[c].getProperty("serviceEndDate").Value.ToString().Substring(6, 4) + services[c].getProperty("serviceEndDate").Value.ToString().Substring(3, 2) + services[c].getProperty("serviceEndDate").Value.ToString().Substring(0, 2));
                    // Testing
                    testingRecord.WriteLine("service_id = " + serviceNodeName + "  |  monday = " + services[c].getProperty("serviceMonday").Value.ToString() + "  |  tuesday = " + services[c].getProperty("serviceTuesday").Value.ToString() + "  |  wednesday = " + services[c].getProperty("serviceWednesday").Value.ToString() + "  |  thursday = " + services[c].getProperty("serviceThursday").Value.ToString() + "  |  friday = " + services[c].getProperty("serviceFriday").Value.ToString() + "  |  saturday = " + services[c].getProperty("serviceSaturday").Value.ToString() + "  |  sunday = " + services[c].getProperty("serviceSunday").Value.ToString() + "  |  start_date = " + services[c].getProperty("serviceStartDate").Value.ToString().Substring(6, 4) + services[c].getProperty("serviceStartDate").Value.ToString().Substring(3, 2) + services[c].getProperty("serviceStartDate").Value.ToString().Substring(0, 2) + "  |  end_date = " + services[c].getProperty("serviceEndDate").Value.ToString().Substring(6, 4) + services[c].getProperty("serviceEndDate").Value.ToString().Substring(3, 2) + services[c].getProperty("serviceEndDate").Value.ToString().Substring(0, 2));
                }
                // Testing
                testingRecord.WriteLine("");


                // Get the calendar_dates.txt information from the Transit Service Calendar Exceptions children nodes and write it to calendarDatesRecord
                Document[] exceptions = new Document(exceptionsRoot).Children;
                // Testing
                testingRecord.WriteLine("calendar_dates.txt");
                for (int d = 0; d < exceptions.Length; d++)
                {
                    // Send the calendar exception to XML
                    XmlNode exceptionsNode = exceptions[d].ToXml(new XmlDocument(), false);
                    // Check to see if the exceptionsNode equals null, if so, the exception is not published
                    string exceptionNodeName;
                    if (exceptionsNode == null)
                    {
                        continue;
                    }
                    else
                    {
                        // Get the calendar exception node name from the xml node attributes
                        exceptionNodeName = exceptionsNode.Attributes.GetNamedItem("nodeName").Value.ToString();
                    }
                    // Check to see if the calendar exception has the removeService option selected
                    if (exceptions[d].getProperty("removeService").Value.ToString() == "-1")
                    {
                        message.Text = message.Text + "&#8226; <span style='color: red;'>Service Calendar Exception <b>" + exceptionNodeName + "</b> does not have a value for Remove Service. To create an exception, you must choose to remove a service.</span><br />";
                        errors++;
                        continue;
                    }
                    // Check to see if the calendar exception has the addService option selected
                    if (exceptions[d].getProperty("addService").Value.ToString() == "-1")
                    {
                        message.Text = message.Text + "&#8226; <span style='color: red;'>Service Calendar Exception <b>" + exceptionNodeName + "</b> does not have a value for Add Service. To create an exception, you must choose to add a service.</span><br />";
                        errors++;
                        continue;
                    }
                    // Check to see if the calendar exception's removeService and addService options are the same, if so pass error
                    if (exceptions[d].getProperty("removeService").Value.ToString() == exceptions[d].getProperty("addService").Value.ToString())
                    {
                        message.Text = message.Text + "&#8226; <span style='color: red;'>Service Calendar Exception <b>" + exceptionNodeName + "</b> has the same value for its Remove Service and Add Service options. The two options have to be different.</span><br />";
                        errors++;
                        continue;
                    }
                    // Write the first line which is the remove service line
                    calendarDatesRecord.WriteLine(exceptions[d].getProperty("removeService").Value.ToString() + "," + exceptions[d].getProperty("exceptionDate").Value.ToString().Substring(6, 4) + exceptions[d].getProperty("exceptionDate").Value.ToString().Substring(3, 2) + exceptions[d].getProperty("exceptionDate").Value.ToString().Substring(0, 2) + ",2");
                    // Write the second line which is the add service line
                    calendarDatesRecord.WriteLine(exceptions[d].getProperty("addService").Value.ToString() + "," + exceptions[d].getProperty("exceptionDate").Value.ToString().Substring(6, 4) + exceptions[d].getProperty("exceptionDate").Value.ToString().Substring(3, 2) + exceptions[d].getProperty("exceptionDate").Value.ToString().Substring(0, 2) + ",1");
                    // Testing
                    testingRecord.WriteLine("service_id = " + exceptions[d].getProperty("removeService").Value.ToString() + "  |  date = " + exceptions[d].getProperty("exceptionDate").Value.ToString().Substring(6, 4) + exceptions[d].getProperty("exceptionDate").Value.ToString().Substring(3, 2) + exceptions[d].getProperty("exceptionDate").Value.ToString().Substring(0, 2) + "  |  exception_type = 2");
                    testingRecord.WriteLine("service_id = " + exceptions[d].getProperty("addService").Value.ToString() + "  |  date = " + exceptions[d].getProperty("exceptionDate").Value.ToString().Substring(6, 4) + exceptions[d].getProperty("exceptionDate").Value.ToString().Substring(3, 2) + exceptions[d].getProperty("exceptionDate").Value.ToString().Substring(0, 2) + "  |  exception_type = 1");
                }
                // Testing
                testingRecord.WriteLine("");


                // Get the trips.txt information from the Transit Trips children nodes and write it to the tripsRecord
                Document[] trips = new Document(tripsRoot).Children;
                // Create the DataSet
                DataSet set = new DataSet("Trip_Stop_Times");
                // Loop through all trips and stop times and put them in a DataSet for using when a trip is based on another
                using (set)
                {
                    for (int m = 0; m < trips.Length; m++)
                    {
                        // Send the trip to XML
                        XmlNode tripsNode = trips[m].ToXml(new XmlDocument(), false);
                        // Check to see if the tripsNode equals null, if so, the trip is not published
                        string tripNodeName;
                        if (tripsNode == null)
                        {
                            continue;
                        }
                        else
                        {
                            // Get the trip node name from the xml node attributes
                            tripNodeName = tripsNode.Attributes.GetNamedItem("nodeName").Value.ToString();
                        }
                        // Check to see if the trip has a route selected
                        if (trips[m].getProperty("route").Value.ToString() == "-1")
                        {
                            continue;
                        }
                        // Check to see if the trip has a calendar selected
                        if (trips[m].getProperty("calendar").Value.ToString() == "-1")
                        {
                            continue;
                        }
                        // Create a table for this trip
                        DataTable table1 = new DataTable(tripNodeName);
                        table1.Columns.Add("tripHeadway");
                        table1.Columns.Add("stop_id");
                        table1.Columns.Add("arrival_time");
                        table1.Columns.Add("departure_time");
                        // Set the trip's stop times to a variable
                        Document[] stopTimes = trips[m].Children;
                        for (int n = 0; n < stopTimes.Length; n++)
                        {
                            // Send the stop time to XML
                            XmlNode stopTimesNode = stopTimes[n].ToXml(new XmlDocument(), false);
                            // Check to see if the stopTimesNode equals null, if so, the stop time is not published
                            string stopTimeNodeName;
                            if (stopTimesNode == null)
                            {
                                continue;
                            }
                            else
                            {
                                // Get the stop time node name from the xml node attributes
                                stopTimeNodeName = stopTimesNode.Attributes.GetNamedItem("nodeName").Value.ToString();
                            }
                            // Check to see if a stop has been selected
                            if (stopTimes[n].getProperty("transitStop").Value.ToString() == "-1")
                            {
                                continue;
                            }
                            // Check to see if a departure time has been given, if not set the departure time to the arrival time
                            string arrivalTime = stopTimes[n].getProperty("arrivalTime").Value.ToString();
                            string departureTime = stopTimes[n].getProperty("departureTime").Value.ToString();
                            if (String.IsNullOrEmpty(departureTime))
                            {
                                departureTime = arrivalTime;
                            }
                            // Create the DataTable row with the correct information for the columns
                            if (String.IsNullOrEmpty(trips[m].getProperty("tripHeadway").Value.ToString()))
                            {
                                table1.Rows.Add(null, stopTimes[n].getProperty("transitStop").Value.ToString(), arrivalTime, departureTime);
                            }
                            else
                            {
                                table1.Rows.Add(trips[m].getProperty("tripHeadway").Value.ToString(), stopTimes[n].getProperty("transitStop").Value.ToString(), arrivalTime, departureTime);
                            }
                        }
                        // Add the DataTables to the DataSet
                        set.Tables.Add(table1);
                    }
                }
                // Testing
                testingRecord.WriteLine("trips.txt");
                // Loop through and do all of the work
                for (int f = 0; f < trips.Length; f++)
                {
                    // Send the trip to XML
                    XmlNode tripsNode = trips[f].ToXml(new XmlDocument(), false);
                    // Check to see if the tripsNode equals null, if so, the trip is not published
                    string tripNodeName;
                    if (tripsNode == null)
                    {
                        continue;
                    }
                    else
                    {
                        // Get the trip node name from the xml node attributes
                        tripNodeName = tripsNode.Attributes.GetNamedItem("nodeName").Value.ToString();
                    }
                    // Check to see if the trip has a route selected
                    if (trips[f].getProperty("route").Value.ToString() == "-1")
                    {
                        message.Text = message.Text + "&#8226; <span style='color: red;'>Trip <b>" + tripNodeName + "</b> does not have a route selected.</span><br />";
                        errors++;
                        continue;
                    }
                    // Check to see if the trip has a calendar selected
                    if (trips[f].getProperty("calendar").Value.ToString() == "-1")
                    {
                        message.Text = message.Text + "&#8226; <span style='color: red;'>Trip <b>" + tripNodeName + "</b> does not have a calendar selected.</span><br />";
                        errors++;
                        continue;
                    }
                    // Set the direction of the trip if it has been given a value
                    string direction;
                    if (String.IsNullOrEmpty(trips[f].getProperty("tripDirection").Value.ToString()))
                    {
                        direction = "";
                    }
                    else
                    {
                        if (trips[f].getProperty("tripDirection").Value.ToString() == "96" || trips[f].getProperty("tripDirection").Value.ToString() == "57")
                        {
                            direction = "0";
                        }
                        else
                        {
                            direction = "1";
                        }
                    }
                    // Testing
                    // message.Text = message.Text + "&#8226; <span style='color: red;'>Trip <b>" + tripNodeName + "</b> direction is: " + trips[f].getProperty("tripDirection").Value.ToString() + ".</span><br />";
                    // Set the trip's stop times to a variable
                    Document[] stopTimes = trips[f].Children;
                    // Check to see if the current trip is based on another.
                    if (trips[f].getProperty("tripBasedOn").Value.ToString() == "-1")
                    {
                        // The trip is not based on another, check to see if there are any stopTimes. If not, is it possible that they accidently meant to base it off of another
                        if (stopTimes.Length == 0)
                        {
                            message.Text = message.Text + "&#8226; <span style='color: red;'>Trip <b>" + tripNodeName + "</b> does not have any stop times provided. Did you mean to base it off of another trip and didn't?</span><br />";
                            errors++;
                            continue;
                        }
                        // Check to see if there is a tripHeadway
                        if (String.IsNullOrEmpty(trips[f].getProperty("tripHeadway").Value.ToString()))
                        {
                            // There is not a tripHeadway, write out the trip
                            tripsRecord.WriteLine(trips[f].getProperty("route").Value.ToString() + "," + trips[f].getProperty("calendar").Value.ToString() + "," + tripNodeName + "," + trips[f].getProperty("tripHeadsign").Value.ToString().Replace(",", " ") + "," + direction + ",,");
                            // Testing
                            testingRecord.WriteLine("");
                            testingRecord.WriteLine("route_id = " + trips[f].getProperty("route").Value.ToString() + "  |  service_id = " + trips[f].getProperty("calendar").Value.ToString() + "  |  trip_id = " + tripNodeName + "  |  trip_headsign = " + trips[f].getProperty("tripHeadsign").Value.ToString().Replace(",", " ") + "  |  direction_id = " + direction + "  |  block_id =  |  shape_id = ");
                            // Loop through each of the stop times and write to the stopTimesRecord
                            for (int g = 0; g < stopTimes.Length; g++)
                            {
                                // Send the stop time to XML
                                XmlNode stopTimesNode = stopTimes[g].ToXml(new XmlDocument(), false);
                                // Check to see if the stopTimesNode equals null, if so, the stop time is not published
                                string stopTimeNodeName;
                                if (stopTimesNode == null)
                                {
                                    continue;
                                }
                                else
                                {
                                    // Get the stop time node name from the xml node attributes
                                    stopTimeNodeName = stopTimesNode.Attributes.GetNamedItem("nodeName").Value.ToString();
                                }
                                // Check to see if a stop has been selected
                                if (stopTimes[g].getProperty("transitStop").Value.ToString() == "-1")
                                {
                                    message.Text = message.Text + "&#8226; <span style='color: red;'>Stop Time <b>" + stopTimeNodeName + "</b> does not have a stop selected.</span><br />";
                                    errors++;
                                    continue;
                                }
                                // Check to see if a departure time has been given, if not set the departure time to the arrival time
                                string arrivalTime = stopTimes[g].getProperty("arrivalTime").Value.ToString();
                                string departureTime = stopTimes[g].getProperty("departureTime").Value.ToString();
                                if (String.IsNullOrEmpty(departureTime))
                                {
                                    departureTime = arrivalTime;
                                }
                                // Write out the stop times
                                stopTimesRecord.WriteLine(tripNodeName + "," + arrivalTime + "," + departureTime + "," + stopTimes[g].getProperty("transitStop").Value.ToString() + "," + (g + 1) + ",,0,0,");
                                // Testing
                                testingRecord.WriteLine("     stop_times.txt:  trip_id = " + tripNodeName + "  |  arrival_time = " + arrivalTime + "  |  departure_time = " + departureTime + "  |  stop_id = " + stopTimes[g].getProperty("transitStop").Value.ToString() + "  |  stop_sequence = " + (g + 1) + "  |  stop_headsign =  |  pickup_type = 0  |  drop_off_type = 0  |  shape_dist_traveled = ");
                            }
                        }
                        else
                        {
                            // There is a tripHeadway
                            // Check to make sure that there is a calendarStartTime and calendarEndTime
                            if (String.IsNullOrEmpty(trips[f].getProperty("calendarStartTime").Value.ToString()) || String.IsNullOrEmpty(trips[f].getProperty("calendarEndTime").Value.ToString()))
                            {
                                message.Text = message.Text + "&#8226; <span style='color: red;'>Trip <b>" + tripNodeName + "</b> has a Trip Headway provided, but does not have a Calendar Start Time or Calendar End Time provided.</span><br />";
                                errors++;
                                continue;
                            }
                            // Create DateTime objects out of the present CalendarStartTime and CalendarEndTime
                            DateTime calendarStartTime = new DateTime(2011, 1, 1, Convert.ToInt32(trips[f].getProperty("calendarStartTime").Value.ToString().Substring(0, 2)), Convert.ToInt32(trips[f].getProperty("calendarStartTime").Value.ToString().Substring(3, 2)), Convert.ToInt32(trips[f].getProperty("calendarStartTime").Value.ToString().Substring(6, 2)));
                            DateTime calendarEndTime = new DateTime(2011, 1, 1, Convert.ToInt32(trips[f].getProperty("calendarEndTime").Value.ToString().Substring(0, 2)), Convert.ToInt32(trips[f].getProperty("calendarEndTime").Value.ToString().Substring(3, 2)), Convert.ToInt32(trips[f].getProperty("calendarEndTime").Value.ToString().Substring(6, 2)));
                            // Find the nextStartTime
                            string lastTime;
                            if (String.IsNullOrEmpty(stopTimes[stopTimes.Length - 1].getProperty("departureTime").Value.ToString()))
                            {
                                lastTime = stopTimes[stopTimes.Length - 1].getProperty("arrivalTime").Value.ToString();
                            }
                            else
                            {
                                lastTime = stopTimes[stopTimes.Length - 1].getProperty("departureTime").Value.ToString();
                            }
                            DateTime lastStopTime = new DateTime(2011, 1, 1, Convert.ToInt32(lastTime.Substring(0, 2)), Convert.ToInt32(lastTime.Substring(3, 2)), Convert.ToInt32(lastTime.Substring(6, 2)));
                            // Create a double variable out of the tripHeadway
                            double headway = Convert.ToInt32(trips[f].getProperty("tripHeadway").Value.ToString());
                            // Add the tripHeadway to the nextStartTime to get the real nextStartTime
                            DateTime nextStartTime = lastStopTime.AddSeconds(headway);
                            // Find the hour difference between the nextStartTime and all of the stopTimes
                            string arrivalTime;
                            string departureTime;
                            int stopTimeHour;
                            int nextStartTimeHour = Convert.ToInt32(nextStartTime.ToString("HH"));
                            int[] hourDifferences = new int[stopTimes.Length];
                            // Testing
                            // testingRecord.WriteLine("");
                            // testingRecord.WriteLine("Stop Time Information:");
                            for (int h = 0; h < stopTimes.Length; h++)
                            {
                                // Send the stop time to XML
                                XmlNode stopTimesNode = stopTimes[h].ToXml(new XmlDocument(), false);
                                // Check to see if a departureTime has been given, if not, set the departureTime to the arrivalTime
                                arrivalTime = stopTimes[h].getProperty("arrivalTime").Value.ToString();
                                departureTime = stopTimes[h].getProperty("departureTime").Value.ToString();
                                if (String.IsNullOrEmpty(departureTime))
                                {
                                    departureTime = arrivalTime;
                                }
                                // Assign the stopTimeHour
                                stopTimeHour = Convert.ToInt32(departureTime.Substring(0, 2));
                                hourDifferences[h] = nextStartTimeHour - stopTimeHour;
                                // Testing
                                //testingRecord.WriteLine("");
                                //testingRecord.WriteLine("CalendarStartTime: " + calendarStartTime.ToString() + " | CalendarEndTime: " + calendarEndTime.ToString() + " | Stop Time: " + departureTime + " | Stop Time Hour: " + stopTimeHour.ToString() + " | Next Start Time of Trip: " + nextStartTime.ToString() + " | Next Start Time Hour: " + nextStartTimeHour.ToString() + " | Hour Difference: " + hourDifferences[h].ToString());
                            }
                            // Create a variable for iteration
                            int sequence = 1;
                            // Grab the first hour of the stopTimes sequence
                            string firstTime;
                            if (String.IsNullOrEmpty(stopTimes[0].getProperty("departureTime").Value.ToString()))
                            {
                                firstTime = stopTimes[0].getProperty("arrivalTime").Value.ToString();
                            }
                            else
                            {
                                firstTime = stopTimes[0].getProperty("departureTime").Value.ToString();
                            }
                            int currentHour = Convert.ToInt32(firstTime.Substring(0, 2));
                            string hasBeenIncreased = "no";
                            // While the nextStartTime is less than the calendarEndTime, loop through the stopTimes and write them and a trip with every pass
                            while (nextStartTime < calendarEndTime)
                            {
                                // Write the trip
                                tripsRecord.WriteLine(trips[f].getProperty("route").Value.ToString() + "," + trips[f].getProperty("calendar").Value.ToString() + "," + tripNodeName + "_" + sequence.ToString() + "," + trips[f].getProperty("tripHeadsign").Value.ToString().Replace(",", " ") + "," + direction + ",,");
                                // Testing
                                testingRecord.WriteLine("");
                                testingRecord.WriteLine("route_id = " + trips[f].getProperty("route").Value.ToString() + "  |  service_id = " + trips[f].getProperty("calendar").Value.ToString() + "  |  trip_id = " + tripNodeName + "_" + sequence.ToString() + "  |  trip_headsign = " + trips[f].getProperty("tripHeadsign").Value.ToString().Replace(",", " ") + "  |  direction_id = " + direction + "  |  block_id =  |  shape_id = ");
                                // Loop through each of the stop times and write to the stopTimesRecord
                                for (int g = 0; g < stopTimes.Length; g++)
                                {
                                    // Send the stop time to XML
                                    XmlNode stopTimesNode = stopTimes[g].ToXml(new XmlDocument(), false);
                                    // Check to see if the stopTimesNode equals null, if so, the stop time is not published
                                    string stopTimeNodeName;
                                    if (stopTimesNode == null)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        // Get the stop time node name from the xml node attributes
                                        stopTimeNodeName = stopTimesNode.Attributes.GetNamedItem("nodeName").Value.ToString().Replace(",", " ");
                                    }
                                    // Check to see if a stop has been selected
                                    if (stopTimes[g].getProperty("transitStop").Value.ToString() == "-1")
                                    {
                                        message.Text = message.Text + "&#8226; <span style='color: red;'>Stop Time <b>" + stopTimeNodeName + "</b> does not have a stop selected.</span><br />";
                                        errors++;
                                        continue;
                                    }
                                    // If the hourDifference is 0, then increase the currentHour
                                    //if (hourDifferences[g] == 0 && hasBeenIncreased == "no")
                                    if (g != 0)
                                    {
                                        if (hourDifferences[g] < hourDifferences[g - 1] && hasBeenIncreased == "no")
                                        {
                                            currentHour++;
                                            hasBeenIncreased = "yes";
                                        }
                                    }
                                    // Check to see if a departure time has been given, if not, set the departure time to the arrival time
                                    string arrivalTime2;
                                    string departureTime2;
                                    arrivalTime2 = currentHour.ToString() + stopTimes[g].getProperty("arrivalTime").Value.ToString().Substring(2, 6);
                                    departureTime2 = stopTimes[g].getProperty("departureTime").Value.ToString();
                                    if (String.IsNullOrEmpty(departureTime2))
                                    {
                                        departureTime2 = arrivalTime2;
                                    }
                                    else
                                    {
                                        departureTime2 = currentHour.ToString() + stopTimes[g].getProperty("departureTime").Value.ToString().Substring(2, 6);
                                    }
                                    // Testing
                                    testingRecord.WriteLine("CurrentHour: " + currentHour.ToString() + " | Has Been Increased: " + hasBeenIncreased);
                                    // Write out the stop times
                                    stopTimesRecord.WriteLine(tripNodeName + "_" + sequence.ToString() + "," + arrivalTime2 + "," + departureTime2 + "," + stopTimes[g].getProperty("transitStop").Value.ToString() + "," + (g + 1) + ",,0,0,");
                                    // Testing
                                    testingRecord.WriteLine("     stop_times.txt:  trip_id = " + tripNodeName + "_" + sequence.ToString() + "  |  arrival_time = " + arrivalTime2 + "  |  departure_time = " + departureTime2 + "  |  stop_id = " + stopTimes[g].getProperty("transitStop").Value.ToString() + "  |  stop_sequence = " + (g + 1) + "  |  stop_headsign =  |  pickup_type = 0  |  drop_off_type = 0  |  shape_dist_traveled = ");
                                }
                                // Set the new hour to be working with
                                nextStartTime = new DateTime(2011, 1, 1, currentHour, Convert.ToInt32(lastTime.Substring(3, 2)), Convert.ToInt32(lastTime.Substring(6, 2)));
                                nextStartTime = nextStartTime.AddSeconds(headway);
                                currentHour = Convert.ToInt32(nextStartTime.ToString("HH"));
                                hasBeenIncreased = "no";
                                // Testing
                                // testingRecord.WriteLine("Next Start Time: " + nextStartTime.ToString());
                                // Increase the sequence variable
                                sequence++;
                            }
                        }
                    }
                    else
                    {
                        // Check to see that the trip has a calendarStartTime and calendarEndTime
                        if (String.IsNullOrEmpty(trips[f].getProperty("calendarStartTime").Value.ToString()) || String.IsNullOrEmpty(trips[f].getProperty("calendarEndTime").Value.ToString()))
                        {
                            message.Text = message.Text + "&#8226; <span style='color: red;'>Trip <b>" + tripNodeName + "</b> is based off of another trip, but does not have a Calendar Start Time or Calendar End Time provided.</span><br />";
                            errors++;
                            continue;
                        }
                        // Check to see if the trip that this trip is based on has a tripHeadway
                        if (DBNull.Value.Equals(set.Tables[trips[f].getProperty("tripBasedOn").Value.ToString()].Rows[0]["tripHeadway"]))
                        {
                            // There is NO tripHeadway in the based on trip
                            // Write out the trip
                            tripsRecord.WriteLine(trips[f].getProperty("route").Value.ToString() + "," + trips[f].getProperty("calendar").Value.ToString() + "," + tripNodeName + "," + trips[f].getProperty("tripHeadsign").Value.ToString().Replace(",", " ") + "," + direction + ",,");
                            // Testing
                            testingRecord.WriteLine("");
                            testingRecord.WriteLine("route_id = " + trips[f].getProperty("route").Value.ToString() + "  |  service_id = " + trips[f].getProperty("calendar").Value.ToString() + "  |  trip_id = " + tripNodeName + "  |  trip_headsign = " + trips[f].getProperty("tripHeadsign").Value.ToString().Replace(",", " ") + "  |  direction_id = " + direction + "  |  block_id =  |  shape_id = ");
                            // Find out how many hours separate the calendarStartTime of the trip and the based on trip's stop times
                            int hourDifference;
                            // Loop through each of the stop times and write to the stopTimesRecord
                            for (int r = 0; r < set.Tables[trips[f].getProperty("tripBasedOn").Value.ToString()].Rows.Count; r++)
                            {
                                // Find the new arrival and departure times created by the trip's calendarStartTime
                                hourDifference = Convert.ToInt32(trips[f].getProperty("calendarStartTime").Value.ToString().Substring(0, 2)) - Convert.ToInt32(set.Tables[trips[f].getProperty("tripBasedOn").Value.ToString()].Rows[r]["arrival_time"].ToString().Substring(0, 2));
                                int newArrivalTimeHour = Convert.ToInt32(set.Tables[trips[f].getProperty("tripBasedOn").Value.ToString()].Rows[r]["arrival_time"].ToString().Substring(0, 2)) + hourDifference;
                                int newDepartureTimeHour = Convert.ToInt32(set.Tables[trips[f].getProperty("tripBasedOn").Value.ToString()].Rows[r]["departure_time"].ToString().Substring(0, 2)) + hourDifference;
                                string newArrivalTime = newArrivalTimeHour.ToString() + set.Tables[trips[f].getProperty("tripBasedOn").Value.ToString()].Rows[r]["arrival_time"].ToString().Substring(2, 6);
                                string newDepartureTime = newDepartureTimeHour.ToString() + set.Tables[trips[f].getProperty("tripBasedOn").Value.ToString()].Rows[r]["departure_time"].ToString().Substring(2, 6);
                                // Write out the stop times
                                stopTimesRecord.WriteLine(tripNodeName + "," + newArrivalTime + "," + newDepartureTime + "," + set.Tables[trips[f].getProperty("tripBasedOn").Value.ToString()].Rows[r]["stop_id"] + "," + (r + 1) + ",,0,0,");
                                // Testing
                                testingRecord.WriteLine("     stop_times.txt:  trip_id = " + tripNodeName + "  |  arrival_time = " + newArrivalTime + "  |  departure_time = " + newDepartureTime + "  |  stop_id = " + set.Tables[trips[f].getProperty("tripBasedOn").Value.ToString()].Rows[r]["stop_id"] + "  |  stop_sequence = " + (r + 1) + "  |  stop_headsign =  |  pickup_type = 0  |  drop_off_type = 0  |  shape_dist_traveled = ");
                            }
                        }
                        else
                        {
                            // There is a tripHeadway in the based on trip
                            // Create DateTime objects out of the present CalendarStartTime and CalendarEndTime
                            DateTime calendarStartTime = new DateTime(2011, 1, 1, Convert.ToInt32(trips[f].getProperty("calendarStartTime").Value.ToString().Substring(0, 2)), Convert.ToInt32(trips[f].getProperty("calendarStartTime").Value.ToString().Substring(3, 2)), Convert.ToInt32(trips[f].getProperty("calendarStartTime").Value.ToString().Substring(6, 2)));
                            DateTime calendarEndTime = new DateTime(2011, 1, 1, Convert.ToInt32(trips[f].getProperty("calendarEndTime").Value.ToString().Substring(0, 2)), Convert.ToInt32(trips[f].getProperty("calendarEndTime").Value.ToString().Substring(3, 2)), Convert.ToInt32(trips[f].getProperty("calendarEndTime").Value.ToString().Substring(6, 2)));
                            // Find out how many stops where in the based on trip
                            int stopTimesLength = set.Tables[trips[f].getProperty("tripBasedOn").Value.ToString()].Rows.Count;
                            // Find the nextStartTime of the based on trip
                            string lastTime;
                            if (String.IsNullOrEmpty(set.Tables[trips[f].getProperty("tripBasedOn").Value.ToString()].Rows[stopTimesLength - 1]["departure_time"].ToString()))
                            {
                                lastTime = set.Tables[trips[f].getProperty("tripBasedOn").Value.ToString()].Rows[stopTimesLength - 1]["arrival_time"].ToString();
                            }
                            else
                            {
                                lastTime = set.Tables[trips[f].getProperty("tripBasedOn").Value.ToString()].Rows[stopTimesLength - 1]["departure_time"].ToString();
                            }
                            DateTime lastStopTime = new DateTime(2011, 1, 1, Convert.ToInt32(lastTime.Substring(0, 2)), Convert.ToInt32(lastTime.Substring(3, 2)), Convert.ToInt32(lastTime.Substring(6, 2)));
                            // Create a double variable out of the tripHeadway of the based on trip
                            double headway = Convert.ToInt32(set.Tables[trips[f].getProperty("tripBasedOn").Value.ToString()].Rows[0]["tripHeadway"]);
                            // Add the tripHeadway to the nextStartTime to get the real nextStartTime
                            DateTime nextStartTime = lastStopTime.AddSeconds(headway);
                            // Find the hourDifferences between the nextStartTime and all of the stopTimes
                            string arrivalTime;
                            string departureTime;
                            int stopTimeHour;
                            int nextStartTimeHour = Convert.ToInt32(nextStartTime.ToString("HH"));
                            int[] hourDifferences = new int[stopTimesLength];
                            // Testing
                            // testingRecord.WriteLine("");
                            // testingRecord.WriteLine("Stop Time Information: ");
                            for (int h = 0; h < stopTimesLength; h++)
                            {
                                arrivalTime = set.Tables[trips[f].getProperty("tripBasedOn").Value.ToString()].Rows[h]["arrival_time"].ToString();
                                departureTime = set.Tables[trips[f].getProperty("tripBasedOn").Value.ToString()].Rows[h]["departure_time"].ToString();
                                // Assign the stopTimeHour
                                stopTimeHour = Convert.ToInt32(departureTime.Substring(0, 2));
                                hourDifferences[h] = nextStartTimeHour - stopTimeHour;
                                // Testing
                                // testingRecord.WriteLine("CalendarStartTime: " + calendarStartTime.ToString() + " | CalendarEndTime: " + calendarEndTime.ToString() + " | Stop Time: " + departureTime + " | Stop Time Hour: " + stopTimeHour.ToString() + " | Next Start Time of Trip: " + nextStartTime.ToString() + " | Next Start Time Hour: " + nextStartTimeHour.ToString() + " | Hour Difference: " + hourDifferences[h].ToString());
                            }
                            // Create a variable for iteration
                            int sequence = 1;
                            // Grab the first hour of the stopTimes sequence
                            string firstTime = set.Tables[trips[f].getProperty("tripBasedOn").Value.ToString()].Rows[0]["departure_time"].ToString();
                            int currentHour = Convert.ToInt32(firstTime.Substring(0, 2));
                            string hasBeenIncreased = "no";
                            // While the nextStartTime is less than the calendarEndTime, loop through the stopTimes and write them and a trip with every pass
                            while (nextStartTime < calendarEndTime)
                            {
                                // Testing
                                testingRecord.WriteLine("");
                                // Create a DateTime object for the next arrivalTime and test that it is in the calendarStartTime and calendarEndTime scope
                                DateTime arrivalTimeTest;
                                // Loop through each of the stop times and write to the stopTimesRecord
                                for (int g = 0; g < stopTimesLength; g++)
                                {
                                    // If the hourDifference is 0, then increase the currentHour
                                    if (g != 0)
                                    {
                                        if (hourDifferences[g] < hourDifferences[g - 1] && hasBeenIncreased == "no")
                                        {
                                            currentHour++;
                                            hasBeenIncreased = "yes";
                                        }
                                    }
                                    // Assign the arrivalTime2 and departureTime2 values
                                    string arrivalTime2;
                                    string departureTime2;
                                    arrivalTime2 = currentHour.ToString() + set.Tables[trips[f].getProperty("tripBasedOn").Value.ToString()].Rows[g]["arrival_time"].ToString().Substring(2, 6);
                                    departureTime2 = currentHour.ToString() + set.Tables[trips[f].getProperty("tripBasedOn").Value.ToString()].Rows[g]["departure_time"].ToString().Substring(2, 6);
                                    // Testing
                                    // testingRecord.WriteLine("CurrentHour: " + currentHour.ToString());
                                    // Set arrivalTimeTest
                                    arrivalTimeTest = new DateTime(2011, 1, 1, currentHour, Convert.ToInt32(set.Tables[trips[f].getProperty("tripBasedOn").Value.ToString()].Rows[0]["arrival_time"].ToString().Substring(3, 2)), Convert.ToInt32(set.Tables[trips[f].getProperty("tripBasedOn").Value.ToString()].Rows[0]["arrival_time"].ToString().Substring(6, 2)));
                                    if (arrivalTimeTest >= calendarStartTime && g != stopTimesLength - 1)
                                    {
                                        // Write out the stop times
                                        stopTimesRecord.WriteLine(tripNodeName + "_" + sequence.ToString() + "," + arrivalTime2 + "," + departureTime2 + "," + set.Tables[trips[f].getProperty("tripBasedOn").Value.ToString()].Rows[g]["stop_id"] + "," + (g + 1) + ",,0,0,");
                                        // Testing
                                        testingRecord.WriteLine("     stop_times.txt:  trip_id = " + tripNodeName + "_" + sequence.ToString() + "  |  arrival_time = " + arrivalTime2 + "  |  departure_time = " + departureTime2 + "  |  stop_id = " + set.Tables[trips[f].getProperty("tripBasedOn").Value.ToString()].Rows[g]["stop_id"] + "  |  stop_sequence = " + (g + 1) + "  |  stop_headsign =  |  pickup_type = 0  |  drop_off_type = 0  |  shape_dist_traveled = ");
                                    }
                                }
                                // Set arrivalTimeTest
                                arrivalTimeTest = new DateTime(2011, 1, 1, currentHour, Convert.ToInt32(set.Tables[trips[f].getProperty("tripBasedOn").Value.ToString()].Rows[0]["arrival_time"].ToString().Substring(3, 2)), Convert.ToInt32(set.Tables[trips[f].getProperty("tripBasedOn").Value.ToString()].Rows[0]["arrival_time"].ToString().Substring(6, 2)));
                                // Testing
                                testingRecord.WriteLine("Arrival Time Test: " + arrivalTimeTest);
                                // Write a variable for when a trip gets written
                                string tripWritten = "no";
                                // Only write the trip if the arrivalTimeTest is within the scope
                                if (arrivalTimeTest >= calendarStartTime)
                                {
                                    // Write the trip
                                    tripsRecord.WriteLine(trips[f].getProperty("route").Value.ToString() + "," + trips[f].getProperty("calendar").Value.ToString() + "," + tripNodeName + "_" + sequence.ToString() + "," + trips[f].getProperty("tripHeadsign").Value.ToString().Replace(",", " ") + "," + direction + ",,");
                                    // Testing
                                    testingRecord.WriteLine("route_id = " + trips[f].getProperty("route").Value.ToString() + "  |  service_id = " + trips[f].getProperty("calendar").Value.ToString() + "  |  trip_id = " + tripNodeName + "_" + sequence.ToString() + "  |  trip_headsign = " + trips[f].getProperty("tripHeadsign").Value.ToString().Replace(",", " ") + "  |  direction_id = " + direction + "  |  block_id =  |  shape_id = ");
                                    testingRecord.WriteLine("");
                                    // Set tripWritten to yes
                                    tripWritten = "yes";
                                }
                                // Set the new hour
                                nextStartTime = new DateTime(2011, 1, 1, currentHour, Convert.ToInt32(lastTime.Substring(3, 2)), Convert.ToInt32(lastTime.Substring(3, 2)));
                                nextStartTime = nextStartTime.AddSeconds(headway);
                                currentHour = Convert.ToInt32(nextStartTime.ToString("HH"));
                                hasBeenIncreased = "no";
                                // Testing
                                // testingRecord.WriteLine("Next Start Time: " + nextStartTime.ToString());
                                // Increase the sequence variable
                                if (tripWritten == "yes")
                                {
                                    sequence++;
                                }
                            }
                        }
                        //testingRecord.WriteLine("");
                        //testingRecord.WriteLine("Table Name: " + set.Tables[tripNodeName] + " | Table that it is based on = " + trips[f].getProperty("tripBasedOn").Value.ToString() + " | Based on Tables's Row Count: = " + set.Tables[trips[f].getProperty("tripBasedOn").Value.ToString()].Rows.Count + " | Based on tables's tripHeadway = " + set.Tables[trips[f].getProperty("tripBasedOn").Value.ToString()].Rows[0]["tripHeadway"]);
                        //testingRecord.WriteLine("");
                    }
                }
                // Testing
                testingRecord.WriteLine("");


                // Get the fare_attributes.txt information from the Transit Fares children nodes and write it to fareAttributesRecord
                Document[] fares = new Document(faresRoot).Children;
                // Testing
                testingRecord.WriteLine("fare_attributes.txt");
                for (int h = 0; h < fares.Length; h++)
                {
                    // Send the fare to XML
                    XmlNode faresNode = fares[h].ToXml(new XmlDocument(), false);
                    // Check to see if the faresNode equals null, if so, the fare is not published
                    string fareNodeName;
                    if (faresNode == null)
                    {
                        continue;
                    }
                    else
                    {
                        // Get the route node name from the xml node attributes
                        fareNodeName = faresNode.Attributes.GetNamedItem("nodeName").Value.ToString();
                    }
                    // Set the transfer option to a value - 0 = No transfers allowed, 1 = 1 transfer allowed, 2 = 2 transfers allowed and empty = Unlimited transfers allowed
                    string transfers;
                    if (fares[h].getProperty("fareTransfers").Value.ToString() == "135")
                    {
                        transfers = "0";
                    }
                    else if (fares[h].getProperty("fareTransfers").Value.ToString() == "136")
                    {
                        transfers = "1";
                    }
                    else if (fares[h].getProperty("fareTransfers").Value.ToString() == "137")
                    {
                        transfers = "2";
                    }
                    else
                    {
                        transfers = "";
                    }
                    // Check to see if a transfer duration has been provided if a transfer is available, if not, return an error
                    if ((transfers == "1" || transfers == "2" || transfers == "") && String.IsNullOrEmpty(fares[h].getProperty("fareTransferDuration").Value.ToString()))
                    {
                        message.Text = message.Text + "&#8226; <span style='color: red;'>Fare <b>" + fareNodeName + "</b> has transfers allowed, but does not have a transfer duration provided.</span><br />";
                        errors++;
                        continue;
                    }
                    // Write the fare attribute information
                    fareAttributesRecord.WriteLine(fareNodeName + "," + fares[h].getProperty("farePrice").Value.ToString() + ",USD,0," + transfers + "," + fares[h].getProperty("fareTransferDuration").Value.ToString());
                    // Testing
                    testingRecord.WriteLine("");
                    testingRecord.WriteLine("fare_id = " + fareNodeName + "  |  price = " + fares[h].getProperty("farePrice").Value.ToString() + "  |  currency_type = USD  |  payment_method = 0  |  transfers = " + transfers + "  |  transfer_duration = " + fares[h].getProperty("fareTransferDuration").Value.ToString());
                    // Check to see if they are using the Routes or Origin and Destination Zones, if they are not using one or the other, return an error
                    if (String.IsNullOrEmpty(fares[h].getProperty("fareRoutes").Value.ToString()) && fares[h].getProperty("fareOriginZone").Value.ToString() == "-1" && fares[h].getProperty("fareDestinationZone").Value.ToString() == "-1")
                    {
                        message.Text = message.Text + "&#8226; <span style='color: red;'>Fare <b>" + fareNodeName + "</b> does not have any Routes selected or does not have an Origin Zone and Destination Zone selected.</span><br />";
                        errors++;
                        continue;
                    }
                    // Check to see if they are using Routes and not Origin and Destination Zones
                    if (!String.IsNullOrEmpty(fares[h].getProperty("fareRoutes").Value.ToString()) && fares[h].getProperty("fareOriginZone").Value.ToString() == "-1" && fares[h].getProperty("fareDestinationZone").Value.ToString() == "-1")
                    {
                        // Take the csv fareRoutes string and separate it by comma
                        string[] fareRoutes = fares[h].getProperty("fareRoutes").Value.ToString().Split(',');
                        // Loop through the fareRoutes string and add a new fare rule to the fareRulesRecord for each
                        for (int g = 0; g < fareRoutes.Length; g++)
                        {
                            // Write the fare rules information
                            fareRulesRecord.WriteLine(fareNodeName + "," + fareRoutes[g] + ",,,");
                            // Testing
                            testingRecord.WriteLine("     fare_rules.txt:  fare_id = " + fareNodeName + "  |  route_id = " + fareRoutes[g] + "  |  origin_id =  |  destination_id =  | contains_id = ");
                        }
                    }
                    // Check to see if they are using Origin and Destination Zones, these options will override the Routes
                    if ((!String.IsNullOrEmpty(fares[h].getProperty("fareRoutes").Value.ToString()) || String.IsNullOrEmpty(fares[h].getProperty("fareRoutes").Value.ToString())) && (fares[h].getProperty("fareOriginZone").Value.ToString() != "-1" || fares[h].getProperty("fareDestinationZone").Value.ToString() != "-1"))
                    {
                        // Check to see that both the Origin Zone and Destination Zone have been selected, if they haven't, return an error
                        if (fares[h].getProperty("fareOriginZone").Value.ToString() == "-1" && fares[h].getProperty("fareDestinationZone").Value.ToString() == "-1")
                        {
                            message.Text = message.Text + "&#8226; <span style='color: red;'>Fare <b>" + fareNodeName + "</b> has a Origin Zone selected or a Destination Zone selected but not both. Both must be selected if that is the option chosen.</span><br />";
                            errors++;
                            continue;
                        }
                        // Write the fare rules information with the Origin and Destination Zone options
                        fareRulesRecord.WriteLine(fareNodeName + ",," + fares[h].getProperty("fareOriginZone").Value.ToString() + "," + fares[h].getProperty("fareDestinationZone").Value.ToString() + ",");
                        // Testing
                        testingRecord.WriteLine("     fare_rules.txt:  fare_id = " + fareNodeName + "  |  route_id =  |  origin_id = " + fares[h].getProperty("fareOriginZone").Value.ToString() + "  |  destination_zone = " + fares[h].getProperty("fareDestinationZone").Value.ToString() + "  |  contains_id = ");
                    }
                }
                // Testing
                testingRecord.WriteLine("");

                // Close and Dispose of all Records and run GC.Collect()
                agencyRecord.Close();
                feedInfoRecord.Close();
                routesRecord.Close();
                stopsRecord.Close();
                calendarsRecord.Close();
                calendarDatesRecord.Close();
                tripsRecord.Close();
                stopTimesRecord.Close();
                fareAttributesRecord.Close();
                fareRulesRecord.Close();
                testingRecord.Close();

                agencyRecord.Dispose();
                feedInfoRecord.Dispose();
                routesRecord.Dispose();
                stopsRecord.Dispose();
                calendarsRecord.Dispose();
                calendarDatesRecord.Dispose();
                tripsRecord.Dispose();
                stopTimesRecord.Dispose();
                fareAttributesRecord.Dispose();
                fareRulesRecord.Dispose();
                testingRecord.Dispose();

                GC.Collect();
            }
            catch (IOException except)
            {
                status.Text = except.Message;
            }

            // If there are no errors, zip up the files and validate the feed
            if (errors > 0)
            {
                status.Text = "<b>Feed NOT Published.</b> Last attempted -- " + DateTime.Now.ToString("dddd, MMMM dd, yyyy hh:mm tt");
                if (errors == 1)
                {
                    message.Text = message.Text + "<br /><b>There is currently " + errors + " error in your transit grouping.</b><br /><br />The Feed Creation has been halted until this error can be resolved.";
                }
                else
                {
                    message.Text = message.Text + "<br /><b>There are currently " + errors + " errors in your transit grouping.</b><br /><br />The Feed Creation has been halted until these errors can be resolved.";
                }
            }
            else
            {
                // Zip up the txt files
                if (!File.Exists(transitDirectory + "\\" + arch))
                {
                    ZipFile output = new ZipFile();
                    output.AddFile(transitDirectory + "\\agency.txt", ".");
                    output.AddFile(transitDirectory + "\\feed_info.txt", ".");
                    output.AddFile(transitDirectory + "\\routes.txt", ".");
                    output.AddFile(transitDirectory + "\\stops.txt", ".");
                    output.AddFile(transitDirectory + "\\calendar.txt", ".");
                    output.AddFile(transitDirectory + "\\calendar_dates.txt", ".");
                    output.AddFile(transitDirectory + "\\trips.txt", ".");
                    output.AddFile(transitDirectory + "\\stop_times.txt", ".");
                    output.AddFile(transitDirectory + "\\fare_attributes.txt", ".");
                    output.AddFile(transitDirectory + "\\fare_rules.txt", ".");
                    output.Name = transitDirectory + "\\" + arch;
                    output.Save();
                    output.Dispose();
                    GC.Collect();
                }
                else
                {
                    ZipFile output = new ZipFile(transitDirectory + "\\" + arch);
                    output.UpdateFile(transitDirectory + "\\agency.txt", ".");
                    output.UpdateFile(transitDirectory + "\\feed_info.txt", ".");
                    output.UpdateFile(transitDirectory + "\\routes.txt", ".");
                    output.UpdateFile(transitDirectory + "\\stops.txt", ".");
                    output.UpdateFile(transitDirectory + "\\calendar.txt", ".");
                    output.UpdateFile(transitDirectory + "\\calendar_dates.txt", ".");
                    output.UpdateFile(transitDirectory + "\\trips.txt", ".");
                    output.UpdateFile(transitDirectory + "\\stop_times.txt", ".");
                    output.UpdateFile(transitDirectory + "\\fare_attributes.txt", ".");
                    output.UpdateFile(transitDirectory + "\\fare_rules.txt", ".");
                    output.Save();
                    output.Dispose();
                    GC.Collect();
                }

                // Feed validation
                try
                {
                    string fileName = transitDirectory + @"\\feedvalidator_googletransit.exe";
                    Process cmdLineProcess = new Process();
                    cmdLineProcess.StartInfo.FileName = fileName;
                    cmdLineProcess.StartInfo.Arguments = "-o " + transitDirectory + "\\error.html -l 9999 " + transitDirectory + "\\" + arch;
                    cmdLineProcess.StartInfo.UseShellExecute = true;
                    cmdLineProcess.StartInfo.CreateNoWindow = true;
                    cmdLineProcess.StartInfo.RedirectStandardOutput = false;
                    cmdLineProcess.StartInfo.RedirectStandardError = false;
                    if (cmdLineProcess.Start())
                    {
                        //litsample1.Text = cmdLineProcess.StandardOutput.ReadToEnd();
                    }
                    else
                    {
                        throw new ApplicationException("Can't read the command line process:" + fileName);
                    }
                }
                catch (ApplicationException except)
                {
                    message.Text = message.Text + except.Message;
                }

                status.Text = "<b>Feed Published Successfully</b> -- " + DateTime.Now.ToString("dddd, MMMM dd, yyyy hh:mm tt");
                message.Text = message.Text + "<br />View the full Feed Validation Report: <a href='/transit/error.html' target='_blank'>Google Feed Validation Report</a><br />View the testing.txt file: <a href='/transit/testing.txt' target='_blank'>testing.txt</a>";
            }

            return;
        }

        private void save_click(object sender, System.Web.UI.ImageClickEventArgs e) { }
    }
}