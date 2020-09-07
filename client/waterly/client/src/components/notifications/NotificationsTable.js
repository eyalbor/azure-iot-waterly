import React, { useState, useEffect } from 'react';
import { renderTime } from '../../actions/timestamp'
import { forwardRef } from 'react';
import {connect} from 'react-redux'
import { updateNotification, fetchNotifications } from '../../actions'

import cloneDeep from 'lodash/cloneDeep';

import CloseIcon from '@material-ui/icons/Close';
import DoneIcon from '@material-ui/icons/Done';
import Grid from '@material-ui/core/Grid'
import MaterialTable from "material-table";
import ArrowDownward from '@material-ui/icons/ArrowDownward';
import Check from '@material-ui/icons/Check';
import ChevronLeft from '@material-ui/icons/ChevronLeft';
import ChevronRight from '@material-ui/icons/ChevronRight';
import Clear from '@material-ui/icons/Clear';
import DeleteOutline from '@material-ui/icons/DeleteOutline';
import FilterList from '@material-ui/icons/FilterList';
import FirstPage from '@material-ui/icons/FirstPage';
import LastPage from '@material-ui/icons/LastPage';
import Remove from '@material-ui/icons/Remove';
import SaveAlt from '@material-ui/icons/SaveAlt';
import Search from '@material-ui/icons/Search';
import VisibilityIcon from '@material-ui/icons/Visibility';
import VisibilityOffIcon from '@material-ui/icons/VisibilityOff';
import ViewColumn from '@material-ui/icons/ViewColumn';
import Alert from '@material-ui/lab/Alert';

const tableIcons = {
    Delete: forwardRef((props, ref) => <DeleteOutline {...props} ref={ref} />),
    Check: forwardRef((props, ref) => <Check {...props} ref={ref} />),
    Clear: forwardRef((props, ref) => <Clear {...props} ref={ref} />),
    DetailPanel: forwardRef((props, ref) => <ChevronRight {...props} ref={ref} />),
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
    CloseIcon: forwardRef((props, ref) => <CloseIcon {...props} ref={ref} />),
    DoneIcon: forwardRef((props, ref) => <DoneIcon {...props} ref={ref} />),
    VisibilityIcon: forwardRef((props, ref) => <VisibilityIcon {...props} ref={ref} />),
    VisibilityOffIcon: forwardRef((props, ref) => <VisibilityOffIcon {...props} ref={ref} />),
};

const columns = [
    {title: "Id", field: "device_id"},
    {title: "Created", field: "created_at", render: rowData => renderTime(rowData.created_at)},
    {title: "Type", field: "type"},
    {title: "Message", field: "message"},
]

class NotificationsTable extends React.Component {

    constructor(props){
        super(props)
        this.state = {selectedRow: null, iserror: false, errorMessages:[]}
    }

    componentDidMount(){
        this.props.fetchNotifications()
    }

    setSelectedRow(selectedRow)
    {
        this.setState({selectedRow})
    }

    setIserror(iserror){
        this.setState({iserror})
    }

    setErrorMessages(errorMessages){
        this.setState({errorMessages})
    }

    // const [data, setData] = useState([]); //table data
    // const [selectedRow, setSelectedRow] = useState(null);

    // //for error handling
    // const [iserror, setIserror] = useState(false)
    // const [errorMessages, setErrorMessages] = useState([])


    submitNotification(row,status){
        let notification = cloneDeep(this.props.notifications[row.tableData.id])
        notification.status = status
        delete notification.tableData
        this.props.updateNotification(notification)
    }

    submitFeedbackNotification(row,feedback){
        let notification = cloneDeep(this.props.notifications[row.tableData.id])
        notification.feedback = feedback
        delete notification.tableData
        this.props.updateNotification(notification)
    }

    render(){
        if(!this.props.isSignedIn){
            return <div>Please signin</div>
        }
        if(!this.props.notifications){
            return <div>Loading...</div>
        }
        return (
            <Grid container>
                <Grid item xs={12}></Grid>
                <Grid item xs={12}>
                <div>
                    {this.state.iserror && 
                    <Alert severity="error">
                        {this.state.errorMessages.map((msg, i) => {
                            return <div key={i}>{msg}</div>
                        })}
                    </Alert>
                    }       
                </div>
                    <MaterialTable
                        title="My Alerts"
                        columns={columns}
                        data={this.props.notifications}
                        icons={tableIcons}
                        options={{
                            sorting: true,
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
                        }}
                        actions={[
                            rowData => ({
                                icon: () => <VisibilityIcon/>,
                                tooltip: "Close",
                                hidden: rowData.status===true,
                                onClick: (event, rowData) => {
                                    console.log("Close " + rowData)
                                    this.submitNotification(rowData, true)
                                }
                            }),
                            rowData => ({
                                icon: () => <VisibilityOffIcon/>,
                                tooltip: "Open",
                                hidden: rowData.status===false,
                                onClick: (event, rowData) => {
                                    console.log("open " + rowData)
                                    this.submitNotification(rowData, false)
                                }
                            }),
                            rowData => ({
                                icon: () => <DoneIcon/>,
                                tooltip: "Accured",
                                hidden: rowData.feedback===true,
                                onClick: (event, rowData) => {
                                    console.log("Close " + rowData)
                                    this.submitFeedbackNotification(rowData, true)
                                }
                            }),
                            rowData => ({
                                icon: () => <CloseIcon/>,
                                tooltip: "Not Accured",
                                hidden: rowData.feedback===false,
                                onClick: (event, rowData) => {
                                    console.log("open " + rowData)
                                    this.submitFeedbackNotification(rowData, false)
                                }
                            })
                        ]}
                    />
                </Grid>
                <Grid item xs={12}></Grid>
                </Grid>
        );
    }
}

//geting list of stream availble inside the component
const mapStateToProps = (state) => {
    //Object.values gets all the object inside and make it as array
    return {
        currentUserId: state.auth.userId,
        isSignedIn: state.auth.isSignedIn,
        ////Object.values gets all the object inside and make it as array
        notifications: Object.values(state.notifications)
    }
}

export default connect(mapStateToProps,
    {updateNotification, fetchNotifications})(NotificationsTable)