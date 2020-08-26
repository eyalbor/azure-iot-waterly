import React from 'react'
import { connect } from 'react-redux'
import EventsTable from './EventTable'
import ScatterEvents from './charts/ScatterEvents'
import { fetchEvents } from '../../actions/index'

class EventsList extends React.Component {

    componentDidMount(){
        this.props.fetchEvents(this.props.deviceId)
    }

    renderList(){
        if(this.props.isSignedIn) {
            //console.log(this.props.currentUserId)
            //this.props.fetchDevices(this.props.currentUserId);
            return (
               <EventsTable data={this.props.events} deviceId={this.props.deviceId}/>
            );
        } else {
            return <h3>Please sign in</h3>
        }
       
    }

    render(){
        return (
            <div className="ui container">
                <h3>Device <u>{this.props.deviceId}</u> Events</h3>
                {this.renderList()}
                <br/>
                <ScatterEvents data={this.props.events}/>
                <br/>
            </div>
        );
    }
};

const mapStateToProps = (state, ownProps) => {
    return {
        deviceId: ownProps.match.params.device_id,
        currentUserId: state.auth.userId,
        isSignedIn: state.auth.isSignedIn,
        events: Object.values(state.events)
    }
}

export default connect(mapStateToProps, {fetchEvents})(EventsList);