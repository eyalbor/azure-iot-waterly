import React from 'react'
import { connect } from 'react-redux'
import EventsTable from './EventTable'
import ScatterEvents from './charts/ScatterEvents'
import { fetchEvents, quality } from '../../actions/index'
import SpeedometerPH from './charts/SpeedometerPH'
import SpeedometerPressure from './charts/SpeedometerPressure'

class EventsList extends React.Component {

    componentDidMount(){
        this.props.fetchEvents(this.props.deviceId)
        this.props.quality(this.props.deviceId);
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
                <div className="ui relaxed centered grid container">
                    <div class="two column centered row">
                        <div className="column">
                            <SpeedometerPH avg={7}/>    
                        </div>
                        <div className="column">
                            <SpeedometerPressure avg={4.3}/>
                        </div>
                    </div>
                </div>
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
        events: Object.values(state.events),
        device_quality: state.quality
    }
}

export default connect(mapStateToProps, {fetchEvents, quality})(EventsList);