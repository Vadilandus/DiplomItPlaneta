//==================================================
// 
//  Copyright 2017 Siemens Product Lifecycle Management Software Inc. All Rights Reserved.
//
//==================================================

using System;

using Teamcenter.ClientX;
using Teamcenter.Schemas.Soa._2006_03.Exceptions;

//Include the Saved Query Service Interface
using Teamcenter.Services.Strong.Query;

// Input and output structures for the service operations 
// Note: the different namespace from the service interface
using Teamcenter.Services.Strong.Query._2006_03.SavedQuery;

using Teamcenter.Services.Strong.Core;
using Teamcenter.Soa.Client.Model;

using ImanQuery = Teamcenter.Soa.Client.Model.Strong.ImanQuery;
using SavedQueriesResponse = Teamcenter.Services.Strong.Query._2007_09.SavedQuery.SavedQueriesResponse;
using QueryInput = Teamcenter.Services.Strong.Query._2008_06.SavedQuery.QueryInput;
using QueryResults = Teamcenter.Services.Strong.Query._2007_09.SavedQuery.QueryResults;
using Teamcenter.Soa.Client.Model.Strong;
using Session = Teamcenter.ClientX.Session;

namespace Teamcenter.Hello
{
    public class Query
    {

        /**
         * Perform a simple query of the database
         * 
         */
        public QueryResults queryItems(string Name, string ItemID)
        {

            ImanQuery query = null;
            // Get the service stub
            SavedQueryService queryService = SavedQueryService.getService(Session.getConnection());
            DataManagementService dmService = DataManagementService.getService(Session.getConnection());
            try
            {

                // *****************************
                // Execute the service operation
                // *****************************
                GetSavedQueriesResponse savedQueries = queryService.GetSavedQueries();
                
                // ищем Запрос с названием "Изделие...+Site_Export"
                for (int i = 0; i < savedQueries.Queries.Length; i++)
                {
                    if (savedQueries.Queries[i].Name.Equals("Item..."))
                    {
                        query = savedQueries.Queries[i].Query;
                        break;

                    }
                }
            }
            catch (ServiceException e)
            {
                Console.Out.WriteLine("GetSavedQueries service request failed.");
                Console.Out.WriteLine(e.Message);
                return null;
            }

            if (query == null)
            {
                Console.WriteLine("There is not an 'Item Name' query.");
                return null;
            }

            try
            {
                QueryInput[] savedQueryInput = new QueryInput[1];
                savedQueryInput[0] = new QueryInput();
                savedQueryInput[0].Query = query;
                savedQueryInput[0].LimitList = new Teamcenter.Soa.Client.Model.ModelObject[0];
                savedQueryInput[0].Entries = new String[] { "Name","Item ID", "Type"};
                if (Name == string.Empty)
                {
                    Name = "*";
                }
                if (ItemID == string.Empty)
                {
                    ItemID = "*";
                }
                if (ItemID == "*" && Name == "*")
                {
                    savedQueryInput[0].MaxNumToReturn = 15;
                }

                Console.WriteLine(ItemID.Length);
                savedQueryInput[0].Values = new String[] { $"{Name}", $"{ItemID}", "H47_Detal" };

                

                //*****************************
                //Execute the service operation
                //*****************************

                SavedQueriesResponse savedQueryResult = queryService.ExecuteSavedQueries(savedQueryInput);
                QueryResults found = savedQueryResult.ArrayOfResults[0];

                return found;
            }
            catch (ServiceException e)
            {
                Console.Out.WriteLine("ExecuteSavedQuery service request failed.");
                Console.Out.WriteLine(e.Message);
                return null;
            }

        }
    }
}
