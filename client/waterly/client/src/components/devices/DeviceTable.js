// https://levelup.gitconnected.com/react-material-table-crud-operations-with-restful-api-data-ca1af738d3c5
//https://material-table.com/#/docs/features/actions
import React, { useState, useEffect } from 'react';
import { forwardRef } from 'react';
import { Link } from 'react-router-dom'
import api from '../../apis/axios'

import { renderTime } from '../../actions/timestamp'

import Grid from '@material-ui/core/Grid'
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
import SyncIcon from '@material-ui/icons/Sync';
import EventIcon from '@material-ui/icons/Event';
import Alert from '@material-ui/lab/Alert';

const tableIcons = {
    Add: forwardRef((props, ref) => <AddBox {...props} ref={ref} />),
    Delete: forwardRef((props, ref) => <DeleteOutline {...props} ref={ref} />),
    Check: forwardRef((props, ref) => <Check {...props} ref={ref} />),
    Clear: forwardRef((props, ref) => <Clear {...props} ref={ref} />),
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
    ViewColumn: forwardRef((props, ref) => <ViewColumn {...props} ref={ref} />),
    SyncIcon: forwardRef((props, ref) => <SyncIcon {...props} ref={ref} />),
    EventIcon: forwardRef((props, ref) => <EventIcon {...props} ref={ref} />),
};

const DeviceTable = ({userId}) => {
    var columns = [
        {title: "Device id", field: "device_id"},
        {title: "Name", field: "name"},
        {title: "Meter Reading [m^3/s]", field: "last_water_read"},
        {title: "Last Update", field: "last_update_timestamp", render: rowData => renderTime(rowData.last_update_timestamp)}
    ]
    const [data, setData] = useState([]); //table data
    const [selectedRow, setSelectedRow] = useState(null);

    //for error handling
    const [iserror, setIserror] = useState(false)
    const [errorMessages, setErrorMessages] = useState([])

    useEffect(() => { 
        api.get(`/devices?userId=${userId}`)
            .then(res => {             
                setData(res.data)
            })
            .catch(error=>{
                console.log("Error")
            })
    }, [])

    const handleRowUpdate = (newData, oldData, resolve) => {
        //validation
        let errorList = []
        if(newData.first_name === ""){
            errorList.push("Please enter first name")
        }
        if(newData.last_name === ""){
            errorList.push("Please enter last name")
        }

        if(errorList.length < 1){
            api.patch("/devices/"+newData.id, newData)
            .then(res => {
                const dataUpdate = [...data];
                const index = oldData.tableData.id;
                dataUpdate[index] = newData;
                setData([...dataUpdate]);
                resolve()
                setIserror(false)
                setErrorMessages([])
            })
            .catch(error => {
                setErrorMessages(["Update failed! Server error"])
                setIserror(true)
                resolve()
                
            })
        }else{
            setErrorMessages(errorList)
            setIserror(true)
            resolve()
        } 
    }

    const handleRowDelete = (oldData, resolve) => {
        console.log("handleRowDelete ",oldData)
        api.delete("/devices/"+oldData.id)
        .then(res => {
            const dataDelete = [...data];
            const index = oldData.tableData.id;
            dataDelete.splice(index, 1);
            setData([...dataDelete]);
            resolve()
        })
        .catch(error => {
            setErrorMessages(["Delete failed! Server error"])
            setIserror(true)
            resolve()
        })
    }


    return (
        <Grid container>
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
                    title="My Devices"
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
                    editable={{
                        onRowDelete: (oldData) =>
                        new Promise((resolve) => {
                            handleRowDelete(oldData, resolve)
                        }),
                    }}
                    actions={[
                        rowData => ({
                            icon: () => <SyncIcon/>,
                            tooltip: "Sync With Device",
                            onClick: (event, rowData) =>
                                console.log("You are sync " + rowData)
                        }),
                        rowData =>({
                            icon: () => <Link style={{ color: '#000' }} to={`/events/device/${rowData.device_id}`}><EventIcon/></Link>,
                            tooltip: "Device Events",
                        }),
                        rowData => ({
                            icon: () => <Link style={{ color: '#000' }} to={`/devices/edit/${rowData.device_id}`}><Edit/></Link>,
                            tooltip: "Edit Device",  
                        }),
                    ]}
                />
            </Grid>
            <Grid item xs={12}></Grid>
            </Grid>
    );
}

export default DeviceTable;