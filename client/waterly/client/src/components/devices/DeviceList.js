import React from 'react'
import { connect } from 'react-redux'
import { Link } from 'react-router-dom'
import { fetchDevices } from '../../actions'

class DeviceList extends React.Component {

    componentDidMount(){
        this.props.fetchDevices();
    }

    renderAdmin(device){
        if(device.userId === this.props.currentUserId){
            return (
                <div className="right floated content">
                <Link to={`/events/${device.id}`} className="ui button">
                        Events
                    </Link>
                    <Link to={`/devices/edit/${device.id}`} className="ui button primary">
                        Edit
                    </Link>
                    <button className="ui button negative">
                        Delete
                    </button>
                </div>
            );
        }
    }

    renderList(){
        if(this.props.isSignedIn){
            return this.props.devices.map(device => {
                    return(
                        <div className="item" key={device.id}>
                            {this.renderAdmin(device)}
                            <i className="large middle aligned icon microchip"/>
                            <div className="content">
                                {device.userId}
                                <div className="name">{device.name}</div>
                            </div>
                        </div>
                    );
                })
            
        } else {
            return <h3>Please sign in</h3>
        }
       
    }

    renderCreate(){
        if(this.props.isSignedIn){
            return(
                <div style={{textAlign:'right'}}>
                    <Link to="/devices/new" className="ui button primary">
                        Create Device
                    </Link>
                </div>
            );
        }
    }

    render() {
        return (
            <div className="ui container">
                <h2>My Devices</h2>
                <div className="ui celled list">{this.renderList()}</div>
                {this.renderCreate()}
            </div>
        );
    }
}

//geting list of stream availble inside the component
const mapStateToProps = (state) => {
    //Object.values gets all the object inside and make it as array
    return {
        devices: Object.values(state.devices),
        currentUserId: state.auth.userId,
        isSignedIn: state.auth.isSignedIn
    }
}

export default connect(mapStateToProps, {fetchDevices})(DeviceList);