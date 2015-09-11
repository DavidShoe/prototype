using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Lesson_202
{
    static public class LessonWebAPI
    {
        const string WebAPIURL = "http://adafruitsample.azurewebsites.net/api?Lesson202=1";

        // This will call an exposed web API at the indicated URL, this will seed the 
        // maker map with an approximation of your location on the world map.
        //
        // See: http://adafruitsample.azurewebsites.net/SamplesMap.html for your pin!
        //
        // The API will return the current time as a string.
        // Example return: "2015-08-31T21:56:25.766Z"
        static public async Task<string> MakeLessonWebApiCall()
        {
            Debug.WriteLine("LessonWebAPI::MakeLessonWebApiCall");

            string responseString = "No response";
            try
            {
                // Prepare the web API call
                WebRequest request = WebRequest.Create(WebAPIURL);

                // Wait for the response notification
                WebResponse response = await request.GetResponseAsync();

                // Read the response string into memory.
                StreamReader streamReader = new StreamReader(response.GetResponseStream());
                responseString = streamReader.ReadToEnd();
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            return responseString;
        }
    }
