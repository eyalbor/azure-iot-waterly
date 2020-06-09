// https://levelup.gitconnected.com/react-material-table-crud-operations-with-restful-api-data-ca1af738d3c5
import React, { useState, useEffect } from 'react';
import { forwardRef } from 'react';
import { renderTime } from '../../actions/timestamp'

import Grid from '@material-ui/core/Grid'
import { makeStyles } from '@material-ui/core/styles';
import MaterialTable from "material-table";
import AddBox from '@material-ui/icons/AddBox';
import ArrowDownward from '@material-ui/icons/ArrowDownward';
import Check from '@material-ui/icons/Check';
import ChevronLeft from '@material-ui/icons/ChevronLeft';
import ChevronRight from '@material-ui/icons/ChevronRight';
import Clear from '@material-ui/icons/Clear';
import DeleteOutline from '@material-ui/icons/DeleteOutline';
import Edit from '@material-ui/icons/Edit';
import FilterList from '@material-ui/icons/FilterList';
import FirstPage from '@material-ui/icons/FirstPage';
import LastPage from '@material-ui/icons/LastPage';
import Remove from '@material-ui/icons/Remove';
import SaveAlt from '@material-ui/icons/SaveAlt';
import Search from '@material-ui/icons/Search';
import ViewColumn from '@material-ui/icons/ViewColumn';
import axios from 'axios'
import Alert from '@material-ui/lab/Alert';

const tableIcons = {
  Add: forwardRef((props, ref) => <AddBox {...props} ref={ref} />),
  Check: forwardRef((props, ref) => <Check {...props} ref={ref} />),
  Clear: forwardRef((props, ref) => <Clear {...props} ref={ref} />),
  Delete: forwardRef((props, ref) => <DeleteOutline {...props} ref={ref} />),
  DetailPanel: forwardRef((props, ref) => <ChevronRight {...props} ref={ref} />),
  Edit: forwardRef((props, ref) => <Edit {...props} ref={ref} />),
  Export: forwardRef((props, ref) => <SaveAlt {...props} ref={ref} />),
  Filter: forwardRef((props, ref) => <FilterList {...props} ref={ref} />),
  FirstPage: forwardRef((props, ref) => <FirstPage {...props} ref={ref} />),
  LastPage: forwardRef((props, ref) => <LastPage {...props} ref={ref} />),
  NextPage: forwardRef((props, ref) => <ChevronRight {...props} ref={ref} />),
  PreviousPage: forwardRef((props, ref) => <ChevronLeft {...props} ref={ref} />),
  ResetSearch: forwardRef((props, ref) => <Clear {...props} ref={ref} />),
  Search: forwardRef((props, ref) => <Search {...props} ref={ref} />),
  SortArrow: forwardRef((props, ref) => <ArrowDownward {...props} ref={ref} />),
  ThirdStateCheck: forwardRef((props, ref) => <Remove {...props} ref={ref} />),
  ViewColumn: forwardRef((props, ref) => <ViewColumn {...props} ref={ref} />)
};

const api = axios.create({
  baseURL: `https://waterly-iot-functions.azurewebsites.net/api`
})

const useStyles = makeStyles((theme) => ({
    root: {
      flexGrow: 1,
    }
  }));

const EventTable = ({deviceId}) => {
    const classes = useStyles();
  var columns = [
    {title: "Id", field: "device_id"},
    {title: "Meter reading", field: "water_read"},
    {title: "Reading time", field: "timestamp", render: rowData => renderTime(rowData.timestamp)}
  ]
  const [data, setData] = useState([]); //table data
  const [selectedRow, setSelectedRow] = useState(null);

  //for error handling
  const [iserror] = useState(false)
  const [errorMessages] = useState([])

  useEffect(() => { 
    api.get(`/events/${deviceId}`)
        .then(res => {            
            console.log(res)   
            setData(res.data)
         })
         .catch(error=>{
             console.log("Error")
         })
  }, [])

  return (
    <div className={classes.root}>
      <Grid container >
          <Grid item xs={12}></Grid>
          <Grid item xs={12}>
          <div>
            {iserror && 
              <Alert severity="error">
                  {errorMessages.map((msg, i) => {
                      return <div key={i}>{msg}</div>
                  })}
              </Alert>
            }       
          </div>
            <MaterialTable
                title="Device Events"
                columns={columns}
                data={data}
                icons={tableIcons}
                onRowClick={((evt, selectedRow) => setSelectedRow(selectedRow.tableData.id))}
                options={{
                    exportButton: true,
                    cellStyle: {
                        width: 20,
                        minWidth: 20
                    },
                    headerStyle: {
                        width:20,
                        minWidth: 20,
                        fontSize: '16px',
                        fontWeight: 'bold'
                    },
                    rowStyle: rowData => ({
                        backgroundColor: (selectedRow === rowData.tableData.id) ? '#EEE' : '#FFF'
                    })
                }}
            />
          </Grid>
          <Grid item xs={12}></Grid>
        </Grid>
    </div>
  );
}

export default EventTable;