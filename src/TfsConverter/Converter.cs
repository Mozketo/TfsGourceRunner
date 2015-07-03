using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace TfsConverter {
    public class Converter {
        /// <summary>
        /// {timestamp}|{user}|{action}|{filepath}|";
        /// </summary>
        public const string LogFormat = "{0}|{1}|{2}|{3}|";

        private static readonly IDictionary<ChangeType, string> AllowedTypes = new Dictionary<ChangeType, string> {
                                                                                       {ChangeType.Add, "A"},
                                                                                       {ChangeType.Edit, "M"},
                                                                                       {ChangeType.Delete, "D"},
                                                                                   };

        private static readonly DateTime UnixBaseDate = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
        private readonly string _username;
        private readonly string _password;
        private readonly string _domain;
        private readonly string _tfsUrl;

        private readonly IDictionary<string, string> _usernameSubstitution = new Dictionary<string, string>() { 
																					{"domain\\user", "Ben"},
                                                                                };

        public Converter(string tfsUrl, string username, string password, string domain = "janison")
        {
            _tfsUrl = tfsUrl;
            _username = username;
            _password = password;
            _domain = domain;
        }

		public void Process(string[] projectPaths, DateTime from, Stream outStream)
		{
			var credential = new System.Net.NetworkCredential(_username, _password, _domain);
			var server = new TfsTeamProjectCollection(new Uri(_tfsUrl), credential);
			server.Authenticate();

			var history = new List<Changeset>();
			foreach (var projectPath in projectPaths)
			{
				var projectPathTemp = projectPath.Trim();

				var source = server.GetService<VersionControlServer>();
				Console.WriteLine("Searching history for project {0}", projectPathTemp);
				var projectHistory = source.QueryHistory(projectPathTemp, VersionSpec.Latest, 0, RecursionType.Full,
														  null, new DateVersionSpec(from), null, int.MaxValue,
														  true,
														  false, false, false).OfType<Changeset>().Reverse().ToList();
				projectHistory = projectHistory.Where(item => item.CreationDate > from).ToList();
				history.AddRange(projectHistory);
			}

			var orderedHistory = history.OrderBy(m => m.CreationDate);

			using (var writer = new StreamWriter(outStream))
			{
				foreach (var item in orderedHistory) // history.ForEach(item =>
				{
					Console.WriteLine("Found changeset id = {0}. Committed: {1}", item.ChangesetId, item.CreationDate);

					String committer = item.Committer;
					if (_usernameSubstitution.ContainsKey(committer.ToLower()))
						committer = _usernameSubstitution[committer.ToLower()];

					foreach (Change change in item.Changes)
					{
						ChangeType changeType = change.ChangeType;
						if (!AllowedTypes.Any(type => (type.Key & changeType) != 0))
							continue;

						KeyValuePair<ChangeType, string> code = AllowedTypes.FirstOrDefault(type => (type.Key & changeType) != 0);
						writer.WriteLine(LogFormat, DateTimeToUnix(item.CreationDate), committer, code.Value,
										 change.Item.ServerItem);
					}
				}
			}
			Console.WriteLine("Processing finished");
		}

        /// <summary>
        /// method for converting a System.DateTime value to a UNIX Timestamp
        /// </summary>
        /// <param name="dateTime">date to convert</param>
        /// <returns></returns>
        private static long DateTimeToUnix(DateTime dateTime)
        {
            //create Timespan by subtracting the value provided from
            //the Unix Epoch
            var span = (dateTime - UnixBaseDate);

            //return the total seconds (which is a UNIX timestamp)
            return (long)span.TotalSeconds;
        }
    }
}